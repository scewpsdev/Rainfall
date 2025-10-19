#include "CGLCompiler.h"

#include "parser/Parser.h"
#include "semantics/Resolver.h"

#include <string>
#include <sstream>
#include <map>
#include <vector>


void CGLCompiler::init(MessageCallback_t msgCallback)
{
	this->msgCallback = msgCallback;
}

void CGLCompiler::terminate()
{
	for (AST::File* file : asts)
		delete file;
	DestroyList(asts);
}

void CGLCompiler::addFile(const char* filename, const char* name, const char* src)
{
	SourceFile sourceFile;
	sourceFile.filename = filename;
	sourceFile.name = name;
	sourceFile.source = src;
	sourceFiles.add(sourceFile);
}

void CGLCompiler::addLinkerFile(const char* filename)
{
	LinkerFile linkerFile;
	linkerFile.filename = filename;
	linkerFiles.add(linkerFile);
}

void CGLCompiler::addLinkerPath(const char* path)
{
	linkerPaths.add(path);
}

bool CGLCompiler::compile()
{
	asts.reserve(sourceFiles.size);

	bool success = true;

	Parser parser(this);
	for (SourceFile& sourceFile : sourceFiles)
	{
		// TODO multithreading
		if (AST::File* ast = parser.run(sourceFile))
			asts.add(ast);
		else
			success = false;
		success = success && !parser.failed;
	}

	//Resolver resolver(this, asts);
	//success = resolver.run() && success;

	return success;
}

class CodegenTCC
{
	CGLCompiler* context;
	AST::File* file = nullptr;

	std::stringstream stream;

	AST::Function* currentFunction = nullptr;
	Scope* scope = nullptr;

	int unnamedLocalId = 0;
	int unnamedGlobalId = 0;

	int indentation = 0;


	void newLine(std::stringstream& stream)
	{
		stream << "\n";
		for (int i = 0; i < indentation; i++)
			stream << "\t";
	}

	void newLine()
	{
		newLine(stream);
	}

	bool isWhitespace(char c)
	{
		return c == ' ' || c == '\t' || c == '\n' || c == '\r';
	}

	void stepBackWhitespace()
	{
		stream.seekg(-1, std::ios::end);
		char c = stream.get();
		if (isWhitespace(c))
		{
			stream.seekp(-1, std::ios::end);
		}
		stream.seekg(0, std::ios::end);
	}

	void stepBackPastWhitespace()
	{
		int i = 1;
		while (true)
		{
			stream.seekg(-i, std::ios::end);
			//instructionStream->seekp(instructionStream->cur-1);
			char c;
			stream.read(&c, 1);
			//char c = instructionStream->get();
			if (isWhitespace(c))
				i++;
			else
			{
				stream.seekp(-i + 1, std::ios::end);
				stream.seekg(0, std::ios::end);
				return;
			}
		}
	}

	void newGlobalName(char name[8])
	{
		name[0] = '_';
		name[1] = '_';
		name[2] = 'G';
		sprintf(&name[3], "%d", unnamedGlobalId++);
	}

	void newLocalName(char name[8])
	{
		name[0] = '_';
		name[1] = '_';
		sprintf(&name[2], "%d", unnamedLocalId++);
	}

	Scope* pushScope()
	{
		Scope* newScope = new Scope();
		newScope->parent = scope;
		scope = newScope;
		return newScope;
	}

	void popScope()
	{
		Scope* lastScope = scope;
		scope = lastScope->parent;
		delete lastScope;
	}

	std::string fpPrecisionToString(FloatingPointPrecision precision)
	{
		switch (precision)
		{
		case FloatingPointPrecision::Half: return "16";
		case FloatingPointPrecision::Single: return "32";
		case FloatingPointPrecision::Double: return "64";
		case FloatingPointPrecision::Decimal: return "80";
		case FloatingPointPrecision::Quad: return "128";
		default:
			SnekAssert(false);
			return "";
		}
	}

	std::string mangleType(TypeID type)
	{
		switch (type->typeKind)
		{
		case AST::TypeKind::Void:
			return "v";
		case AST::TypeKind::Integer:
			return std::string(type->integerType.isSigned ? "i" : "u") + std::to_string(type->integerType.bitWidth);
		case AST::TypeKind::FloatingPoint:
			return "f" + fpPrecisionToString(type->fpType.precision);
		case AST::TypeKind::Boolean:
			return "b";
		case AST::TypeKind::String:
			return "s";
		case AST::TypeKind::Any:
			return "y";
		case AST::TypeKind::Struct:
			return "x" + std::string(type->structType.name);
		case AST::TypeKind::Class:
			return "X" + std::string(type->classType.name);
		case AST::TypeKind::Alias:
			return mangleType(type->aliasType.alias);
		case AST::TypeKind::Pointer:
			return "p" + mangleType(type->pointerType.elementType);
		case AST::TypeKind::Optional:
			return "o" + mangleType(type->optionalType.elementType);
		case AST::TypeKind::Function:
		{
			std::string result = "F" + mangleType(type->functionType.returnType) + std::to_string(type->functionType.numParams);
			for (int i = 0; i < type->functionType.numParams; i++)
				result = result + mangleType(type->functionType.paramTypes[i]);
			return result;
		}
		case AST::TypeKind::Tuple:
		{
			std::string result = "t" + std::to_string(type->tupleType.numValues);
			for (int i = 0; i < type->tupleType.numValues; i++)
				result = result + mangleType(type->tupleType.valueTypes[i]);
			return result;
		}
		case AST::TypeKind::Array:
			if (type->arrayType.length == -1)
				return "a" + mangleType(type->arrayType.elementType);
			else
				return "a" + std::to_string(type->arrayType.length) + mangleType(type->arrayType.elementType);
		default:
			SnekAssert(false);
			return "";
		}
	}

	std::string genTypeVoid()
	{
		return "void";
	}

	std::string genTypeInteger(TypeID type)
	{
		return "";
	}

	std::string genTypeFloatingPoint(TypeID type)
	{
		switch (type->fpType.precision)
		{
		case FloatingPointPrecision::Half:
			return "f32";
		case FloatingPointPrecision::Single:
			return "float";
		case FloatingPointPrecision::Double:
			return "f64";
		case FloatingPointPrecision::Decimal:
			return "f64";
		case FloatingPointPrecision::Quad:
			return "f64";
		default:
			SnekAssert(false);
			return "";
		}
	}

	std::string genTypeBoolean()
	{
		return "bool";
	}

	std::string genTypeAny()
	{
		return "any";
	}

	void importStruct(AST::Struct* strct)
	{
		genStruct(strct);
		return;

		stream << "struct " << strct->mangledName;

		if (strct->hasBody)
		{
			stream << "{";
			indentation++;
			newLine();

			genStruct(strct);

			stepBackWhitespace();
			indentation--;
			stream << "}";
		}
		else
		{
			stream << ";";
		}
	}

	void importClass(AST::Class* clss)
	{
		genClass(clss);
		return;
	}

	std::string genTypeStruct(TypeID type)
	{
		return "struct " + std::string(type->structType.name);
	}

	std::string genTypeClass(TypeID type)
	{
		return "struct " + std::string(type->structType.name) + "*";
	}

	std::string genTypeAlias(TypeID type)
	{
		return genType(type->aliasType.alias);
	}

	std::string genTypePointer(TypeID type)
	{
		return genType(type->pointerType.elementType) + "*";
	}

	std::string genTypeOptional(TypeID type)
	{
		return "";
	}

	std::string genTypeFunction(TypeID type)
	{
		return "";
	}

	std::string genTypeTuple(TypeID type)
	{
		return "";
	}

	std::string genTypeArray(TypeID type)
	{
		return "";
	}

	std::string genTypeString()
	{
		return "string";
	}

	std::string genType(TypeID type)
	{
		switch (type->typeKind)
		{
		case AST::TypeKind::Void:
			return genTypeVoid();
		case AST::TypeKind::Integer:
			return genTypeInteger(type);
		case AST::TypeKind::FloatingPoint:
			return genTypeFloatingPoint(type);
		case AST::TypeKind::Boolean:
			return genTypeBoolean();
		case AST::TypeKind::Any:
			return genTypeAny();
		case AST::TypeKind::NamedType:
			SnekAssert(false);
			return "";
		case AST::TypeKind::Struct:
			return genTypeStruct(type);
		case AST::TypeKind::Class:
			return genTypeClass(type);
		case AST::TypeKind::Alias:
			return genTypeAlias(type);
		case AST::TypeKind::Pointer:
			return genTypePointer(type);
		case AST::TypeKind::Optional:
			return genTypeOptional(type);
		case AST::TypeKind::Function:
			return genTypeFunction(type);
		case AST::TypeKind::Tuple:
			return genTypeTuple(type);
		case AST::TypeKind::Array:
			return genTypeArray(type);
		case AST::TypeKind::String:
			return genTypeString();
		default:
			SnekAssert(false);
			return "";
		}
	}

	std::string genType(AST::Type* type)
	{
		switch (type->typeKind)
		{
		case AST::TypeKind::Void:
			return "void";
		case AST::TypeKind::Integer:
			return "int";
		case AST::TypeKind::FloatingPoint:
			return "float";
		case AST::TypeKind::Boolean:
			return "bool";
		case AST::TypeKind::NamedType:
			return ((AST::NamedType*)type)->name;
		case AST::TypeKind::Vector:
			return (((AST::VectorType*)type)->integer ? "i" : "") + std::string("vec") + std::to_string(((AST::VectorType*)type)->size);
		case AST::TypeKind::Matrix:
			return "mat" + std::to_string(((AST::MatrixType*)type)->size);
		case AST::TypeKind::Sampler:
			return std::string("sampler") + (((AST::SamplerType*)type)->cubemap ? "Cube" : "2D");
		default:
			SnekAssert(false);
			return "";
		}
	}

	std::string arrayGetElement(std::string array, int index, AST::Expression* ast)
	{
		if (ast && ast->isConstant())
		{
			SnekAssert(ast->type == AST::ExpressionType::InitializerList);
			AST::InitializerList* initializerList = (AST::InitializerList*)ast;
			return genExpression(initializerList->values[index]);
		}
		else
		{
			return array + ".buffer[" + std::to_string(index) + "]";
		}
	}

	std::string castValue(std::string expression, TypeID valueType, TypeID type, AST::Expression* ast = nullptr)
	{
		if (CompareTypes(valueType, type))
			return expression;
		else if (valueType->typeKind == AST::TypeKind::Integer && type->typeKind == AST::TypeKind::Integer)
		{
			return "(" + genType(type) + ")(" + expression + ")";
		}
		else if (valueType->typeKind == AST::TypeKind::FloatingPoint && type->typeKind == AST::TypeKind::Integer)
		{
			return "(" + genType(type) + ")(" + expression + ")";
		}
		else if (valueType->typeKind == AST::TypeKind::Integer && type->typeKind == AST::TypeKind::FloatingPoint)
		{
			return "(" + genType(type) + ")(" + expression + ")";
		}
		else if (valueType->typeKind == AST::TypeKind::FloatingPoint && type->typeKind == AST::TypeKind::FloatingPoint)
		{
			return "(" + genType(type) + ")(" + expression + ")";
		}
		else if (valueType->typeKind == AST::TypeKind::Pointer && type->typeKind == AST::TypeKind::Pointer)
		{
			if (/*argIsConstant || */valueType->pointerType.elementType->typeKind == AST::TypeKind::Void || type->pointerType.elementType->typeKind == AST::TypeKind::Void)
				return "(" + genType(type) + ")(" + expression + ")";
			SnekAssert(false);
			return "";
		}
		else if (valueType->typeKind == AST::TypeKind::Pointer && valueType->pointerType.elementType->typeKind == AST::TypeKind::Void &&
			type->typeKind == AST::TypeKind::Optional)
		{
			return "(" + genType(type) + ")" + expression + "";
		}
		else if (valueType->typeKind != AST::TypeKind::Optional && type->typeKind == AST::TypeKind::Optional &&
			CompareTypes(type->optionalType.elementType, valueType))
		{
			return "(" + genType(type) + "){" + expression + ",1}";
		}
		else if (valueType->typeKind == AST::TypeKind::Array && type->typeKind == AST::TypeKind::Array)
		{
			if (CompareTypes(valueType->arrayType.elementType, type->arrayType.elementType))
			{
				if (valueType->arrayType.length != -1 && type->arrayType.length == -1)
					return "(" + genType(type) + "){" + expression + ".buffer," + std::to_string(valueType->arrayType.length) + "}";
				SnekAssert(false);
				return "";
			}
			else
			{
				SnekAssert(valueType->arrayType.length != -1);
				if (type->arrayType.length != -1)
				{
					SnekAssert(valueType->arrayType.length == type->arrayType.length);
					std::stringstream stream;
					stream << "{";
					for (int i = 0; i < valueType->arrayType.length; i++)
					{
						stream << castValue(arrayGetElement(expression, i, ast), valueType->arrayType.elementType, type->arrayType.elementType);
						if (i < type->arrayType.length - 1)
							stream << ",";
					}
					stream << "}";
					return stream.str();
				}
				else
				{
					std::stringstream stream;
					stream << "&{";
					for (int i = 0; i < valueType->arrayType.length; i++)
					{
						stream << castValue(expression + ".buffer[" + std::to_string(i) + "]", valueType->arrayType.elementType, type->arrayType.elementType);
						if (i < type->arrayType.length - 1)
							stream << ",";
					}
					stream << "}," << valueType->arrayType.length;
					return stream.str();
				}
			}
		}
		// Boolean conversions
		else if (type->typeKind == AST::TypeKind::Boolean && valueType->typeKind == AST::TypeKind::Integer)
		{
			return expression;
		}
		else if (type->typeKind == AST::TypeKind::Boolean && valueType->typeKind == AST::TypeKind::Pointer)
		{
			return expression;
		}
		else if (type->typeKind == AST::TypeKind::Boolean && valueType->typeKind == AST::TypeKind::Optional)
		{
			return expression + ".hasValue";
		}
		else if (type->typeKind == AST::TypeKind::Any)
		{
			TypeID valueTypeExtended = valueType;
			if (valueType->typeKind == AST::TypeKind::Integer)
				valueTypeExtended = GetIntegerType(64, true);
			else if (valueType->typeKind == AST::TypeKind::FloatingPoint)
				valueTypeExtended = GetFloatingPointType(FloatingPointPrecision::Double);
			else if (valueType->typeKind == AST::TypeKind::Boolean)
				valueTypeExtended = GetIntegerType(64, false);

			std::string value = expression;

			if (!(ast && ast->lvalue))
			{
				char valueVariableName[8];
				newLocalName(valueVariableName);

				stream << genType(valueTypeExtended) << ' ' << valueVariableName << '=' << value << ';';
				newLine(stream);

				value = valueVariableName;
			}

			switch (valueType->typeKind)
			{
			case AST::TypeKind::Any:
			case AST::TypeKind::Struct:
			case AST::TypeKind::Class:
			case AST::TypeKind::Optional:
			case AST::TypeKind::Function:
			case AST::TypeKind::Tuple:
			case AST::TypeKind::Array:
			case AST::TypeKind::String:
			{
				/*
				char valuePointerName[8];
				newLocalName(valuePointerName);

				*instructionStream << genType(GetPointerType(valueTypeExtended)) << ' ' << valuePointerName << "=&" << value << ';';
				newLine(*instructionStream);

				value = std::string(valuePointerName);
				*/

				return "(any){(void*)" + std::to_string((int)valueType->typeKind) + ",(void*)&" + value + "}";
			}
			default:
				return "(any){(void*)" + std::to_string((int)valueType->typeKind) + ",*(void**)&" + value + "}";
			}
		}
		else if (valueType->typeKind == AST::TypeKind::Any)
		{
			switch (type->typeKind)
			{
			case AST::TypeKind::Integer:
				return "(" + genType(type) + ")*(i64*)(&" + expression + ".value)";
			case AST::TypeKind::FloatingPoint:
				return "(" + genType(type) + ")*(f64*)(&" + expression + ".value)";
			case AST::TypeKind::Boolean:
				return "(" + genType(type) + ")*(u64*)(&" + expression + ".value)";
			case AST::TypeKind::Pointer:
				return "*(" + genType(type) + "*)(&" + expression + ".value)";
			case AST::TypeKind::Any:
			case AST::TypeKind::Struct:
			case AST::TypeKind::Class:
			case AST::TypeKind::Optional:
			case AST::TypeKind::Function:
			case AST::TypeKind::Tuple:
			case AST::TypeKind::Array:
			case AST::TypeKind::String:
				return "*(" + genType(type) + "*)(" + expression + ".value)";
			default:
				SnekAssert(false);
				return "";
			}
		}
		else if (valueType->typeKind == AST::TypeKind::Pointer &&
			valueType->pointerType.elementType->typeKind == AST::TypeKind::Integer &&
			valueType->pointerType.elementType->integerType.bitWidth == 8 &&
			type->typeKind == AST::TypeKind::String)
		{
			if (ast && ast->type == AST::ExpressionType::StringLiteral)
				return "(string){" + expression + "," + std::to_string(((AST::StringLiteral*)ast)->length) + "}";
			else
				return "(string){" + expression + ",strlen(" + expression + ")}";
		}
		else if (valueType->typeKind == AST::TypeKind::Array &&
			valueType->arrayType.elementType->typeKind == AST::TypeKind::Integer &&
			valueType->arrayType.elementType->integerType.bitWidth == 8 &&
			type->typeKind == AST::TypeKind::String)
		{
			return "(string){" + expression + (valueType->arrayType.length != -1 ? ".buffer" : ".ptr") + "," + (valueType->arrayType.length != -1 ? std::to_string(valueType->arrayType.length) : expression + ".length") + "}";
		}
		// Conversions
		else if (valueType->typeKind == AST::TypeKind::String &&
			type->typeKind == AST::TypeKind::Integer)
		{
			return "__sto" + std::string(type->integerType.isSigned ? "i" : "u") + std::to_string(type->integerType.bitWidth) + "(" + expression + ")";
		}
		else if (valueType->typeKind == AST::TypeKind::Integer &&
			type->typeKind == AST::TypeKind::String)
		{
			return "__itos(" + expression + ")";
		}
		else
		{
			SnekAssert(false);
			return "";
		}
	}

	std::string castValue(AST::Expression* expression, TypeID type)
	{
		return castValue(genExpression(expression), expression->valueType, type, expression);
	}

	std::string genExpressionIntegerLiteral(AST::IntegerLiteral* expression)
	{
		return std::to_string(expression->value);
	}

	std::string genExpressionFloatingPointLiteral(AST::FloatingPointLiteral* expression)
	{
		if (expression->isDouble)
			return expression->valueStr;
		else
			return std::string(expression->valueStr) + "f";
	}

	std::string genExpressionBooleanLiteral(AST::BooleanLiteral* expression)
	{
		return std::to_string(expression->value);
	}

	std::string genExpressionCharacterLiteral(AST::CharacterLiteral* expression)
	{
		if (expression->value == '\n')
			return "'\\n'";
		else if (expression->value == '\r')
			return "'\\r'";
		else if (expression->value == '\t')
			return "'\\t'";
		else if (expression->value == '\\')
			return "'\\\\'";
		else if (expression->value == '\0')
			return "'\\0'";
		else
			return std::to_string(expression->value);
	}

	std::string genExpressionNullLiteral(AST::NullLiteral* expression)
	{
		return "0";
	}

	void writeStringLiteral(const char* str, int length, std::stringstream& stream)
	{
		stream << "\"";
		for (int i = 0; i < length; i++)
		{
			char c = str[i];
			if (c == '\n')
				stream << "\\n";
			else if (c == '\r')
				stream << "\\r";
			else if (c == '\t')
				stream << "\\t";
			else if (c == '\\')
				stream << "\\\\";
			else if (c == '\0')
				stream << "\\0";
			else if (c == '"')
				stream << "\\\"";
			else
				stream << c;
		}
		stream << "\"";
	}

	void writeStringLiteral(const char* str, std::stringstream& stream)
	{
		writeStringLiteral(str, (int)strlen(str), stream);
	}

	std::string genExpressionStringLiteral(AST::StringLiteral* expression)
	{
		return "";
	}

	std::string getVariableValue(Variable* variable)
	{
		return "";
	}

	std::string getFunctionValue(AST::Function* function)
	{
		return function->mangledName;
	}

	std::string genExpressionInitializerList(AST::InitializerList* expression)
	{
		SnekAssert(expression->initializerType);
		if (expression->initializerType->typeKind == AST::TypeKind::Array)
		{
			if (expression->initializerType->arrayType.length != -1)
			{
				std::stringstream result;
				result << "(" << genType(expression->initializerType) << "){{";
				for (int i = 0; i < expression->values.size; i++)
				{
					result << castValue(expression->values[i], expression->initializerType->arrayType.elementType);
					if (i < expression->values.size - 1)
						result << ",";
				}
				result << "}}";
				return result.str();
			}
			else
			{
				std::stringstream arrayValueStream;
				arrayValueStream << "{";
				for (int i = 0; i < expression->values.size; i++)
				{
					arrayValueStream << castValue(expression->values[i], expression->initializerType->arrayType.elementType);
					if (i < expression->values.size - 1)
						arrayValueStream << ",";
				}
				arrayValueStream << "};";
				newLine(arrayValueStream);

				char arrayName[8];
				newLocalName(arrayName);
				stream << genType(expression->initializerType->arrayType.elementType) << " " << arrayName << "[" << expression->values.size << "]=" << arrayValueStream.str();

				return "{" + std::string(arrayName) + "," + std::to_string(expression->values.size) + "}";
			}
		}
		else
		{
			return "";
		}
	}

	std::string genExpressionIdentifier(AST::Identifier* expression)
	{
		return expression->name;

		if (expression->variable)
		{
			return getVariableValue(expression->variable);
		}
		else if (expression->functions.size > 0)
		{
			//SnekAssert(expression->functions.size == 1);
			return getFunctionValue(expression->functions[0]);
		}
		else
		{
			SnekAssert(false);
			return "";
		}
	}

	std::string genExpressionCompound(AST::CompoundExpression* expression)
	{
		return "(" + genExpression(expression->value) + ")";
	}

	std::string genExpressionTuple(AST::TupleExpression* expression)
	{
		std::stringstream result;
		result << "(" << genType(expression->valueType) << "){";
		for (int i = 0; i < expression->values.size; i++)
		{
			AST::Expression* value = expression->values[i];
			result << genExpression(value);
			if (i < expression->values.size - 1)
				result << ",";
		}
		result << "}";
		return result.str();
	}

	std::string genExpressionFunctionCall(AST::FunctionCall* expression)
	{
		std::stringstream callStream;
		callStream << genExpression(expression->callee) << "(";
		for (int i = 0; i < expression->arguments.size; i++)
		{
			callStream << genExpression(expression->arguments[i]);
			if (i < expression->arguments.size - 1)
				callStream << ", ";
		}
		callStream << ")";
		return callStream.str();

		if (expression->isCast)
		{
			SnekAssert(expression->arguments.size == 1);
			return castValue(expression->arguments[0], expression->castDstType);
		}

		//auto parentStream = currentStream;
		//currentStream = &callStream;

		std::string returnValue = "";

		List<AST::Expression*> arguments;
		arguments.addAll(expression->arguments);
		if (expression->methodInstance)
			arguments.insert(0, expression->methodInstance);

		SnekAssert(expression->callee->valueType->typeKind == AST::TypeKind::Function);
		if (expression->callee->valueType->functionType.returnType->typeKind != AST::TypeKind::Void)
		{
			char localName[8];
			newLocalName(localName);
			callStream << genType(expression->callee->valueType->functionType.returnType) << " " << localName << "=";
			returnValue = localName;
		}

		std::string callee = expression->function ? getFunctionValue(expression->function) : genExpression(expression->callee);
		callStream << callee << "(";
		int numParams = expression->callee->valueType->functionType.numParams;
		for (int i = 0; i < numParams; i++)
		{
			callStream << castValue(arguments[i], expression->callee->valueType->functionType.paramTypes[i]);;

			if (i < numParams - 1)
				callStream << ",";
		}

		if (expression->callee->valueType->functionType.varArgs)
		{
			int numVarArgs = arguments.size - numParams;

			TypeID varArgsType = expression->callee->valueType->functionType.varArgsType;
			TypeID arrayType = GetArrayType(varArgsType, -1);

			if (numVarArgs)
			{
				AST::Expression* lastArg = arguments[arguments.size - 1];
				if (numVarArgs == 1 && lastArg->valueType->typeKind == AST::TypeKind::Array && CompareTypes(lastArg->valueType->arrayType.elementType, varArgsType))
				{
					callStream << "," << castValue(lastArg, arrayType);
				}
				else
				{
					std::stringstream varArgsStream;
					varArgsStream << "{";
					for (int i = 0; i < numVarArgs; i++)
					{
						varArgsStream << castValue(arguments[numParams + i], varArgsType);
						if (i < numVarArgs - 1)
							varArgsStream << ',';
					}
					varArgsStream << "};";
					newLine(varArgsStream);

					char varArgArrayName[8];
					newLocalName(varArgArrayName);
					stream << genType(varArgsType) << ' ' << varArgArrayName << '[' << numVarArgs << "]=" << varArgsStream.str();

					callStream << ",(" << genType(arrayType) << "){" << varArgArrayName << ',' << numVarArgs << '}';
				}
			}
			else
			{
				callStream << ",(" << genType(arrayType) << "){(void*)0," << numVarArgs << '}';
			}
		}
		callStream << ");";
		newLine(callStream);

		//currentStream = parentStream;

		stream << callStream.str();

		return returnValue;
	}

	void assert(std::string expr, AST::SourceLocation& location)
	{
		stream << "assert(" << expr << ",";
		writeStringLiteral(location.filename, stream);
		stream << "," << location.line << "," << location.col << ");";
		newLine();
	}

	void arrayBoundsCheck(std::string arr, TypeID arrayType, std::string index, AST::SourceLocation& location)
	{
		assert(index + ">=0&&" + index + "<" + (arrayType->arrayType.length != -1 ? std::to_string(arrayType->arrayType.length) : arr + ".length"), location);
		//*instructionStream << "assert(" << index << ">=0&&" << index << "<";
		//if (arrayType->arrayType.length != -1)
		//	*instructionStream << arrayType->arrayType.length;
		//else
		//	*instructionStream << arr << ".length";
		//*instructionStream << ");";
	}

	void stringBoundsCheck(std::string str, std::string index, AST::SourceLocation& location)
	{
		assert(index + ">=0&&" + index + "<" + str + ".length", location);
	}

	std::string genExpressionSubscriptOperator(AST::SubscriptOperator* expression)
	{
		std::string operand = genExpression(expression->operand);
		if (expression->operand->valueType->typeKind == AST::TypeKind::Pointer)
		{
			SnekAssert(expression->arguments.size == 1);
			return operand + "[" + genExpression(expression->arguments[0]) + "]";
		}
		else if (expression->operand->valueType->typeKind == AST::TypeKind::Array)
		{
			SnekAssert(expression->arguments.size == 1);
			std::string index = genExpression(expression->arguments[0]);
			arrayBoundsCheck(operand, expression->operand->valueType, index, expression->location);
			if (expression->operand->valueType->arrayType.length != -1)
				return operand + ".buffer[" + index + "]";
			else
				return operand + ".ptr[" + index + "]";
		}
		else if (expression->operand->valueType->typeKind == AST::TypeKind::String)
		{
			SnekAssert(expression->arguments.size == 1);
			std::string index = genExpression(expression->arguments[0]);
			stringBoundsCheck(operand, index, expression->location);
			return operand + ".ptr[" + index + "]";
		}
		else if (expression->operand->valueType->typeKind == AST::TypeKind::Struct || expression->operand->valueType->typeKind == AST::TypeKind::Class)
		{
			SnekAssert(expression->operatorOverload);
			std::stringstream stream;
			stream << getFunctionValue(expression->operatorOverload) << "(";
			stream << genExpression(expression->operand) << ",";
			for (int i = 0; i < expression->arguments.size; i++)
			{
				stream << genExpression(expression->arguments[i]);
				if (i < expression->arguments.size - 1)
					stream << ",";
			}
			stream << ")";
			return stream.str();
		}
		else
		{
			SnekAssert(false);
			return "";
		}
	}

	std::string genExpressionDotOperator(AST::DotOperator* expression)
	{
		return genExpression(expression->operand) + "." + expression->name;

		if (expression->module)
		{
			// TODO
			SnekAssert(false);
			return "";
		}
		else if (expression->namespacedVariable)
			return getVariableValue(expression->namespacedVariable);
		else if (expression->namespacedFunctions.size > 0)
			return getFunctionValue(expression->namespacedFunctions[0]);
		else if (expression->builtinTypeProperty != AST::BuiltinTypeProperty::Null)
		{
			switch (expression->builtinTypeProperty)
			{
			case AST::BuiltinTypeProperty::Int8Min: return "__INT8_MIN";
			case AST::BuiltinTypeProperty::Int8Max: return "__INT8_MAX";
			case AST::BuiltinTypeProperty::Int16Min: return "__INT16_MIN";
			case AST::BuiltinTypeProperty::Int16Max: return "__INT16_MAX";
			case AST::BuiltinTypeProperty::Int32Min: return "__INT32_MIN";
			case AST::BuiltinTypeProperty::Int32Max: return "__INT32_MAX";
			case AST::BuiltinTypeProperty::Int64Min: return "__INT64_MIN";
			case AST::BuiltinTypeProperty::Int64Max: return "__INT64_MAX";
			case AST::BuiltinTypeProperty::UInt8Min: return "__UINT8_MIN";
			case AST::BuiltinTypeProperty::UInt8Max: return "__UINT8_MAX";
			case AST::BuiltinTypeProperty::UInt16Min: return "__UINT16_MIN";
			case AST::BuiltinTypeProperty::UInt16Max: return "__UINT16_MAX";
			case AST::BuiltinTypeProperty::UInt32Min: return "__UINT32_MIN";
			case AST::BuiltinTypeProperty::UInt32Max: return "__UINT32_MAX";
			case AST::BuiltinTypeProperty::UInt64Min: return "__UINT64_MIN";
			case AST::BuiltinTypeProperty::UInt64Max: return "__UINT64_MAX";
			default:
				SnekAssert(false);
				return "";
			}
		}
		else if (expression->structField)
		{
			if (expression->operand->valueType->typeKind == AST::TypeKind::Pointer && expression->operand->valueType->pointerType.elementType->typeKind == AST::TypeKind::Struct)
				return genExpression(expression->operand) + "->" + expression->structField->name;
			else if (expression->operand->valueType->typeKind == AST::TypeKind::Struct)
				return genExpression(expression->operand) + "." + expression->structField->name;
			else
			{
				SnekAssert(false);
				return "";
			}
		}
		else if (expression->classMethod)
			return expression->classMethod->mangledName;
		else if (expression->classField)
			return genExpression(expression->operand) + "->" + expression->classField->name;
		else if (expression->fieldIndex != -1)
		{
			if (expression->operand->valueType->typeKind == AST::TypeKind::Optional)
			{
				if (expression->fieldIndex == 0)
					return genExpression(expression->operand) + ".value";
				else if (expression->fieldIndex == 1)
					return genExpression(expression->operand) + ".hasValue";
				else
				{
					SnekAssert(false);
					return "";
				}
			}
			else if (expression->operand->valueType->typeKind == AST::TypeKind::Tuple)
			{
				SnekAssert(expression->fieldIndex >= 0 && expression->fieldIndex < expression->operand->valueType->tupleType.numValues);
				return genExpression(expression->operand) + "._" + std::to_string(expression->fieldIndex);
			}
			else if (expression->operand->valueType->typeKind == AST::TypeKind::Array)
			{
				if (expression->operand->valueType->arrayType.length == -1)
				{
					if (expression->fieldIndex == 0)
						return genExpression(expression->operand) + ".length";
					else if (expression->fieldIndex == 1)
						return genExpression(expression->operand) + ".buffer";
					else
					{
						SnekAssert(false);
						return "";
					}
				}
				else
				{
					if (expression->fieldIndex == 0)
						return std::to_string(expression->operand->valueType->arrayType.length);
					else if (expression->fieldIndex == 1)
						return genExpression(expression->operand);
					else
					{
						SnekAssert(false);
						return "";
					}
				}
			}
			else if (expression->operand->valueType->typeKind == AST::TypeKind::String)
			{
				if (expression->fieldIndex == 0)
					return genExpression(expression->operand) + ".length";
				else if (expression->fieldIndex == 1)
					return genExpression(expression->operand) + ".ptr";
				else
				{
					SnekAssert(false);
					return "";
				}
			}
			else if (expression->operand->valueType->typeKind == AST::TypeKind::Any)
			{
				if (expression->fieldIndex == 0) // type
					return "(u64)" + genExpression(expression->operand) + ".type";
				else if (expression->fieldIndex == 1) // value
					return genExpression(expression->operand) + ".value";
				else
				{
					SnekAssert(false);
					return "";
				}
			}
			else
			{
				SnekAssert(false);
				return "";
			}
		}
		else
		{
			SnekAssert(false);
			return "";
		}
	}

	std::string genExpressionTypecast(AST::Typecast* expression)
	{
		return castValue(expression->value, expression->dstType->typeID);
	}

	std::string genExpressionSizeof(AST::Sizeof* expression)
	{
		return "";
	}

	std::string genExpressionMalloc(AST::Malloc* expression)
	{
		if (expression->dstType->typeKind == AST::TypeKind::Array && ((AST::ArrayType*)expression->dstType)->length && expression->malloc)
		{
			AST::ArrayType* arrayType = (AST::ArrayType*)expression->dstType;
			std::string elementType = genType(arrayType->elementType);
			std::string length = genExpression(arrayType->length);
			return "(" + genType(arrayType->typeID) + "){(" + elementType + "*)__alloc(sizeof(" + elementType + ")*" + length + ")," + length + "}";
		}
		else
		{
			std::string type = genType(expression->dstType);
			if (expression->dstType->typeKind == AST::TypeKind::Class)
				type = type.substr(0, type.length() - 1);
			std::string count = "";
			if (expression->count)
			{
				count = "*(" + genExpression(expression->count) + ")";
			}
			return "(" + type + "*" + ")__alloc(sizeof(" + type + ")" + count + ")";
		}
	}

	std::string genExpressionUnaryOperator(AST::UnaryOperator* expression)
	{
		switch (expression->operatorType)
		{
		case AST::UnaryOperatorType::Not:
			SnekAssert(!expression->position);
			return "!" + genExpression(expression->operand);
		case AST::UnaryOperatorType::Negate:
			SnekAssert(!expression->position);
			return "-" + genExpression(expression->operand);
		case AST::UnaryOperatorType::Reference:
			SnekAssert(!expression->position);
			return "&" + genExpression(expression->operand);
		case AST::UnaryOperatorType::Dereference:
			SnekAssert(!expression->position);
			return "*" + genExpression(expression->operand);
		case AST::UnaryOperatorType::Increment:
		{
			std::string operand = genExpression(expression->operand);
			if (!expression->position)
			{
				stream << operand << "+=1;";
				newLine();
				return operand;
			}
			else
			{
				char initialValueName[8];
				newLocalName(initialValueName);
				stream << genType(expression->operand->valueType) << " " << initialValueName << "=" << operand << ";";
				newLine();
				stream << operand << "+=1;";
				newLine();
				return initialValueName;
			}
		}
		case AST::UnaryOperatorType::Decrement:
		{
			std::string operand = genExpression(expression->operand);
			if (!expression->position)
			{
				stream << operand << "-=1;";
				newLine();
				return operand;
			}
			else
			{
				char initialValueName[8];
				newLocalName(initialValueName);
				stream << genType(expression->operand->valueType) << " " << initialValueName << "=" << operand << ";";
				newLine();
				stream << operand << "-=1;";
				newLine();
				return initialValueName;
			}
		}
		default:
			SnekAssert(false);
			return "";
		}
		return "";
	}

	std::string genExpressionBinaryOperator(AST::BinaryOperator* expression)
	{
		auto left = genExpression(expression->left);
		auto right = genExpression(expression->right);

		switch (expression->operatorType)
		{
		case AST::BinaryOperatorType::Add:
			return castValue(left, expression->left->valueType, expression->valueType) + "+" + castValue(right, expression->right->valueType, expression->valueType);
		case AST::BinaryOperatorType::Subtract:
			return castValue(left, expression->left->valueType, expression->valueType) + "-" + castValue(right, expression->right->valueType, expression->valueType);
		case AST::BinaryOperatorType::Multiply:
			return castValue(left, expression->left->valueType, expression->valueType) + "*" + castValue(right, expression->right->valueType, expression->valueType);
		case AST::BinaryOperatorType::Divide:
			return castValue(left, expression->left->valueType, expression->valueType) + "/" + castValue(right, expression->right->valueType, expression->valueType);
		case AST::BinaryOperatorType::Modulo:
			return castValue(left, expression->left->valueType, expression->valueType) + "%" + castValue(right, expression->right->valueType, expression->valueType);
		case AST::BinaryOperatorType::Equals:
		{
			if (expression->left->valueType->typeKind == AST::TypeKind::Optional &&
				expression->right->valueType->typeKind == AST::TypeKind::Pointer && expression->right->valueType->pointerType.elementType->typeKind == AST::TypeKind::Void)
			{
				SnekAssert(dynamic_cast<AST::NullLiteral*>(expression->right));
				return "!" + left + ".hasValue";
			}
			else
			{
				return left + "==" + right;
			}
		}
		case AST::BinaryOperatorType::DoesNotEqual:
			if (expression->left->valueType->typeKind == AST::TypeKind::Optional &&
				expression->right->valueType->typeKind == AST::TypeKind::Pointer && expression->right->valueType->pointerType.elementType->typeKind == AST::TypeKind::Void)
			{
				SnekAssert(dynamic_cast<AST::NullLiteral*>(expression->right));
				return left + ".hasValue";
			}
			else
			{
				return left + "!=" + right;
			}
		case AST::BinaryOperatorType::LessThan:
			return left + "<" + right;
		case AST::BinaryOperatorType::GreaterThan:
			return left + ">" + right;
		case AST::BinaryOperatorType::LessThanEquals:
			return left + "<=" + right;
		case AST::BinaryOperatorType::GreaterThanEquals:
			return left + ">=" + right;
		case AST::BinaryOperatorType::LogicalAnd:
			return left + "&&" + right;
		case AST::BinaryOperatorType::LogicalOr:
			return left + "||" + right;
		case AST::BinaryOperatorType::BitwiseAnd:
		case AST::BinaryOperatorType::BitwiseOr:
		case AST::BinaryOperatorType::BitwiseXor:
		case AST::BinaryOperatorType::BitshiftLeft:
		case AST::BinaryOperatorType::BitshiftRight:
		case AST::BinaryOperatorType::Assignment:
		{
			return left + "=" + right;
		}
		case AST::BinaryOperatorType::PlusEquals:
		case AST::BinaryOperatorType::MinusEquals:
		case AST::BinaryOperatorType::TimesEquals:
		case AST::BinaryOperatorType::DividedByEquals:
		case AST::BinaryOperatorType::ModuloEquals:
		case AST::BinaryOperatorType::BitwiseAndEquals:
		case AST::BinaryOperatorType::BitwiseOrEquals:
		case AST::BinaryOperatorType::BitwiseXorEquals:
		case AST::BinaryOperatorType::BitshiftLeftEquals:
		case AST::BinaryOperatorType::BitshiftRightEquals:
		{
			std::string op;
			if (expression->operatorType == AST::BinaryOperatorType::PlusEquals) op = "+";
			else if (expression->operatorType == AST::BinaryOperatorType::MinusEquals) op = "-";
			else if (expression->operatorType == AST::BinaryOperatorType::TimesEquals) op = "*";
			else if (expression->operatorType == AST::BinaryOperatorType::DividedByEquals) op = "/";
			else if (expression->operatorType == AST::BinaryOperatorType::ModuloEquals) op = "%";
			else if (expression->operatorType == AST::BinaryOperatorType::BitwiseAndEquals) op = "&";
			else if (expression->operatorType == AST::BinaryOperatorType::BitwiseOrEquals) op = "|";
			else if (expression->operatorType == AST::BinaryOperatorType::BitwiseXorEquals) op = "^";
			else if (expression->operatorType == AST::BinaryOperatorType::BitshiftLeftEquals) op = "<<";
			else if (expression->operatorType == AST::BinaryOperatorType::BitshiftRightEquals) op = ">>";
			else SnekAssert(false);
			stream << left << "=" << castValue(left + op + right, expression->opAssignResultingType, expression->left->valueType) << ";";
			newLine();
			return "";
		}
		case AST::BinaryOperatorType::ReferenceAssignment:
		case AST::BinaryOperatorType::NullCoalescing:
		{
			return left + ".hasValue?" + left + ".value:" + castValue(right, expression->right->valueType, expression->left->valueType->optionalType.elementType);
		}
		default:
			SnekAssert(false);
			return "";
		}
	}

	std::string genExpressionTernaryOperator(AST::TernaryOperator* expression)
	{
		char val[8];
		newLocalName(val);
		stream << genType(expression->thenValue->valueType) << " " << val << ";";
		newLine();
		stream << "if(" << genExpression(expression->condition) << "){";
		std::string then = genExpression(expression->thenValue);
		stream << val << "=" << then << ";}else{";
		std::string els = genExpression(expression->elseValue);
		stream << val << "=" << els << ";}";
		newLine();
		return std::string(val);
	}

	std::string genExpression(AST::Expression* expression)
	{
		switch (expression->type)
		{
		case AST::ExpressionType::IntegerLiteral:
			return genExpressionIntegerLiteral((AST::IntegerLiteral*)expression);
		case AST::ExpressionType::FloatingPointLiteral:
			return genExpressionFloatingPointLiteral((AST::FloatingPointLiteral*)expression);
		case AST::ExpressionType::BooleanLiteral:
			return genExpressionBooleanLiteral((AST::BooleanLiteral*)expression);
		case AST::ExpressionType::CharacterLiteral:
			return genExpressionCharacterLiteral((AST::CharacterLiteral*)expression);
		case AST::ExpressionType::NullLiteral:
			return genExpressionNullLiteral((AST::NullLiteral*)expression);
		case AST::ExpressionType::StringLiteral:
			return genExpressionStringLiteral((AST::StringLiteral*)expression);
		case AST::ExpressionType::InitializerList:
			return genExpressionInitializerList((AST::InitializerList*)expression);
		case AST::ExpressionType::Identifier:
			return genExpressionIdentifier((AST::Identifier*)expression);
		case AST::ExpressionType::Compound:
			return genExpressionCompound((AST::CompoundExpression*)expression);
		case AST::ExpressionType::Tuple:
			return genExpressionTuple((AST::TupleExpression*)expression);
		case AST::ExpressionType::FunctionCall:
			return genExpressionFunctionCall((AST::FunctionCall*)expression);
		case AST::ExpressionType::SubscriptOperator:
			return genExpressionSubscriptOperator((AST::SubscriptOperator*)expression);
		case AST::ExpressionType::DotOperator:
			return genExpressionDotOperator((AST::DotOperator*)expression);
		case AST::ExpressionType::Typecast:
			return genExpressionTypecast((AST::Typecast*)expression);
		case AST::ExpressionType::Sizeof:
			return genExpressionSizeof((AST::Sizeof*)expression);
		case AST::ExpressionType::Malloc:
			return genExpressionMalloc((AST::Malloc*)expression);
		case AST::ExpressionType::UnaryOperator:
			return genExpressionUnaryOperator((AST::UnaryOperator*)expression);
		case AST::ExpressionType::BinaryOperator:
			return genExpressionBinaryOperator((AST::BinaryOperator*)expression);
		case AST::ExpressionType::TernaryOperator:
			return genExpressionTernaryOperator((AST::TernaryOperator*)expression);
		default:
			SnekAssert(false);
			return "";
		}
	}

	void genStatementCompound(AST::CompoundStatement* statement)
	{
		//*instructionStream << "{";
		//indentation++;
		//newLine();

		//pushScope();
		for (int i = 0; i < statement->statements.size; i++)
		{
			genStatement(statement->statements[i]);
		}
		//popScope();

		//instructionStream->seekp(-1, instructionStream->cur);
		//indentation--;
		//newLine();
		//*instructionStream << "}";
	}

	char getLastNonWhitespaceChar(std::stringstream& stream)
	{
		int i = 1;
		while (true)
		{
			stream.seekg(-i++, std::ios::end);
			char c = stream.get();
			if (!isWhitespace(c))
			{
				stream.seekg(0, std::ios::beg);
				return c;
			}
		}
		SnekAssert(false);
		return 0;
	}

	void genStatementExpression(AST::ExpressionStatement* statement)
	{
		stream << genExpression(statement->expression) << ";";
		newLine();
		/*
		std::string value = genExpression(statement->expression);
		if (value.size() > 0)
		{
			*instructionStream << value << ';';
			newLine();
		}
		*/
	}

	void genStatementVariableDeclaration(AST::VariableDeclaration* statement)
	{
		//std::stringstream* parentStream = instructionStream;
		std::stringstream stream;
		//instructionStream = &stream;

		if (statement->isConstant)
			stream << "const ";

		TypeID varType = statement->varType;
		int pointerDepth = 0;
		while (varType->typeKind == AST::TypeKind::Pointer)
		{
			pointerDepth++;
			varType = varType->pointerType.elementType;
		}

		stream << genType(varType) << " ";

		for (int i = 0; i < statement->declarators.size; i++)
		{
			AST::VariableDeclarator* declarator = statement->declarators[i];

			for (int j = 0; j < pointerDepth; j++)
				stream << '*';
			stream << declarator->name;
			if (declarator->value)
			{
				stream << "=" << castValue(declarator->value, statement->varType);
			}
			else
			{
				stream << "={0}";
			}
			if (i < statement->declarators.size - 1)
				stream << ",";
		}
		stream << ";";

		newLine();
	}

	void genStatementIf(AST::IfStatement* statement)
	{
		auto condition = genExpression(statement->condition);

		stream << "if(" << castValue(condition, statement->condition->valueType, GetBoolType()) << "){";

		indentation++;
		newLine();

		genStatement(statement->thenStatement);
		if (statement->elseStatement)
		{
			if (statement->elseStatement->type == AST::StatementType::If)
			{
				stepBackWhitespace();
				indentation--;

				stream << "}else ";
				genStatement(statement->elseStatement);
			}
			else
			{
				stepBackWhitespace();
				indentation--;
				//newLine();

				stream << "}else{";

				indentation++;
				newLine();

				genStatement(statement->elseStatement);

				stepBackWhitespace();
				indentation--;
				//newLine();

				stream << "}";
			}
		}
		else
		{
			stepBackWhitespace();
			indentation--;
			//newLine();

			stream << "}";
		}

		newLine();
	}

	void genStatementWhile(AST::WhileLoop* statement)
	{
		stream << "while(1){";

		std::string cond = genExpression(statement->condition);
		cond = castValue(cond, statement->condition->valueType, GetBoolType());

		stream << "if(!(" << cond << "))break;";

		indentation++;
		newLine();

		genStatement(statement->body);
		stepBackWhitespace();
		indentation--;
		//newLine();

		stream << "}";

		newLine();
	}

	void genStatementFor(AST::ForLoop* statement)
	{
		/*
		*currentStream
			<< "for(int " << statement->iteratorName->name << "=" << genExpression(statement->startValue) << ";"
			<< statement->iteratorName->name << "<" << genExpression(statement->endValue) << ";"
			<< statement->iteratorName->name << "++)";
			*/

		if (!statement->iteratorName)
		{
			stream << "{";

			if (statement->initStatement)
			{
				genStatement(statement->initStatement);
				//stepBackPastWhitespace();
				//instructionStream->seekp(-2, instructionStream->cur);
			}

			stream << "while(";

			if (statement->conditionExpr)
				stream << genExpression(statement->conditionExpr);
			stream << "){";

			indentation++;
			newLine();

			genStatement(statement->body);

			if (statement->iterateExpr)
			{
				genExpression(statement->iterateExpr);
				//newLine();
			}

			stepBackWhitespace();
			indentation--;

			stream << "}}";
		}
		else
		{
			std::string container = genExpression(statement->container);

			stream << "for(int __it=0;__it<";
			if (statement->container->valueType->typeKind == AST::TypeKind::Array)
			{
				if (statement->container->valueType->arrayType.length != -1)
					stream << statement->container->valueType->arrayType.length;
				else
					stream << container << ".length";
			}
			else
			{
				SnekAssert(false);
			}

			stream << ";__it++){";

			indentation++;
			newLine();

			stream << genType(statement->container->valueType->arrayType.elementType) << " const " << statement->iteratorName << "=";

			if (statement->container->valueType->typeKind == AST::TypeKind::Array)
			{
				if (statement->container->valueType->arrayType.length != -1)
					stream << container << ".buffer[__it]";
				else
					stream << container << ".ptr[__it]";
			}
			else
			{
				SnekAssert(false);
			}

			stream << ";";
			newLine();

			indentation++;
			newLine();

			genStatement(statement->body);

			stepBackWhitespace();
			indentation--;

			stream << "}";
		}

		newLine();
	}

	void genStatementBreak(AST::Break* statement)
	{
		stream << "break;";
		newLine();
	}

	void genStatementContinue(AST::Continue* statement)
	{
		stream << "continue;";
		newLine();
	}

	void genStatementReturn(AST::Return* statement)
	{
		if (statement->value)
		{
			SnekAssert(currentFunction->functionType->functionType.returnType->typeKind != AST::TypeKind::Void);
			std::string value = castValue(statement->value, currentFunction->functionType->functionType.returnType);
			stream << "return " << value << ";";
			newLine();
		}
		else
		{
			stream << "return;";
			newLine();
		}
	}

	void genStatementAssert(AST::Assert* statement)
	{
		stream << "if(!(" << castValue(statement->condition, GetBoolType()) << ")){";
		indentation++;
		newLine();

		if (statement->message)
		{
			char message[128] = {};
			sprintf(message, "Assertion failed at %s:%d:%d: \"%%s\"\n", statement->file->name, statement->location.line, statement->location.col);

			stream << "printf(";
			writeStringLiteral(message, stream);
			stream << ", " << genExpression(statement->message) << ".ptr);";
		}
		else
		{
			char message[128] = {};
			sprintf(message, "Assertion failed at %s:%d:%d\n", statement->file->name, statement->location.line, statement->location.col);

			stream << "printf(";
			writeStringLiteral(message, stream);
			stream << ");";
		}
		newLine();

		stream << "__asm__ volatile(\"int $0x03\");";
		indentation--;
		newLine();

		stream << "}";
		newLine();
	}

	void freeValue(std::string value, TypeID type)
	{
		if (type->typeKind == AST::TypeKind::Pointer)
		{
			stream << "__free(" + value + ");";
			newLine();
		}
		else if (type->typeKind == AST::TypeKind::String)
		{
			stream << "__free(" + value + ".ptr);";
			newLine();
		}
		else if (type->typeKind == AST::TypeKind::Array)
		{
			TypeID elementType = type->arrayType.elementType;
			if (elementType->typeKind == AST::TypeKind::String || elementType->typeKind == AST::TypeKind::Array)
			{
				char itName[8];
				newLocalName(itName);

				std::string length = type->arrayType.length != -1 ? std::to_string(type->arrayType.length) : value + ".length";

				stream << "for(int " << itName << "=0;" << itName << "<" << length << ";" << itName << "++){";
				newLine();

				freeValue(value + (type->arrayType.length != -1 ? ".buffer[" : ".ptr[") + itName + "]", elementType);

				stream << "}";
				newLine();
			}

			if (type->arrayType.length == -1)
				stream << "__free(" + value + ".ptr);";
			newLine();
		}
	}

	void genStatementFree(AST::Free* statement)
	{
		for (int i = 0; i < statement->values.size; i++)
		{
			AST::Expression* value = statement->values[i];
			freeValue(genExpression(value), value->valueType);
		}
	}

	void genStatement(AST::Statement* statement)
	{
		switch (statement->type)
		{
		case AST::StatementType::NoOp:
			break;
		case AST::StatementType::Compound:
			genStatementCompound((AST::CompoundStatement*)statement);
			break;
		case AST::StatementType::Expression:
			genStatementExpression((AST::ExpressionStatement*)statement);
			break;
		case AST::StatementType::VariableDeclaration:
			genStatementVariableDeclaration((AST::VariableDeclaration*)statement);
			break;
		case AST::StatementType::If:
			genStatementIf((AST::IfStatement*)statement);
			break;
		case AST::StatementType::While:
			genStatementWhile((AST::WhileLoop*)statement);
			break;
		case AST::StatementType::For:
			genStatementFor((AST::ForLoop*)statement);
			break;
		case AST::StatementType::Break:
			genStatementBreak((AST::Break*)statement);
			break;
		case AST::StatementType::Continue:
			genStatementContinue((AST::Continue*)statement);
			break;
		case AST::StatementType::Return:
			genStatementReturn((AST::Return*)statement);
			break;
		case AST::StatementType::Defer:
		case AST::StatementType::Assert:
			genStatementAssert((AST::Assert*)statement);
			break;
		case AST::StatementType::Free:
			genStatementFree((AST::Free*)statement);
			break;
		default:
			SnekAssert(false);
			break;
		}
	}

	void genStruct(AST::Struct* strct)
	{
		stream << "struct " << strct->name << " {";
		indentation++;
		newLine();

		for (int i = 0; i < strct->fields.size; i++)
		{
			stream << genType(strct->fields[i]->type) << " " << strct->fields[i]->name << ";";
			newLine();
		}

		stepBackWhitespace();
		indentation--;
		stream << "};";
		stream << "\n\n";
	}

	void genClass(AST::Class* clss)
	{
		if (clss->isGeneric)
		{
			for (int i = 0; i < clss->genericInstances.size; i++)
			{
				genClass(clss->genericInstances[i]);
			}
			return;
		}


		stream << "struct " << clss->mangledName;
		stream << "{";
		indentation++;
		newLine();

		for (int i = 0; i < clss->fields.size; i++)
		{
			stream << genType(clss->fields[i]->type) << " " << clss->fields[i]->name << ";";
			newLine();
		}

		stepBackWhitespace();
		indentation--;
		stream << "};";
		stream << "\n\n";

		//types << structStream.str();
	}

	void genFunctionHeader(AST::Function* function)
	{
		if (function->isEntryPoint)
			return;

		if (function->isGeneric)
		{
			for (int i = 0; i < function->genericInstances.size; i++)
			{
				genFunctionHeader(function->genericInstances[i]);
			}
			return;
		}

		// Apparently not needed for functions?
		//if (HasFlag(function->flags, AST::DeclarationFlags::DllImport))
		//	stream << "__declspec(dllimport) ";

		stream << genType(function->functionType->functionType.returnType) << " " << function->mangledName << "(";
		for (int i = 0; i < function->functionType->functionType.numParams; i++)
		{
			stream << genType(function->functionType->functionType.paramTypes[i]) << " " << function->paramNames[i];
			if (i < function->functionType->functionType.numParams - 1)
				stream << ",";
		}
		stream << ");\n\n";

		//functionDeclarations << functionStream.str();
	}

	void genFunction(AST::Function* function)
	{
		if (function->isEntryPoint)
		{
			stream << "int";
		}
		else
		{
			stream << genType(function->returnType);
		}

		stream << " " << function->name << "(";
		for (int i = 0; i < function->paramTypes.size; i++)
		{
			if (function->paramFlags[i] & 1)
				stream << "in ";
			if (function->paramFlags[i] & 2)
				stream << "out ";
			stream << genType(function->paramTypes[i]) << " " << function->paramNames[i];
			if (i < function->paramTypes.size - 1)
				stream << ",";
		}
		stream << ")";

		unnamedLocalId = 0;

		stream << " {";
		indentation++;
		newLine();
		pushScope();

		currentFunction = function;

		genStatement(function->body);
		if (function->isEntryPoint)
		{
			stream << "return 0;";
			newLine();
		}

		currentFunction = nullptr;

		popScope();
		stepBackWhitespace();
		//instructionStream->seekp(-1, instructionStream->cur);
		indentation--;
		//newLine();
		stream << "}";
		stream << "\n\n";

		//if (function->body || function->bodyExpression)
		//	functions << functionStream.str();
		//else
		//	functionDeclarations << functionStream.str();
	}

	void genTypedef(AST::Typedef* td)
	{
		stream << "typedef " << genType(td->alias) << " " << td->name << ";";
		newLine();

		//types << typedefStream.str();
	}

	void genGlobal(AST::GlobalVariable* global)
	{
		if (HasFlag(global->flags, AST::DeclarationFlags::Constant) &&
			(global->varType->typeKind == AST::TypeKind::Integer || global->varType->typeKind == AST::TypeKind::FloatingPoint || global->varType->typeKind == AST::TypeKind::Boolean))
		{
			for (int i = 0; i < global->declarators.size; i++)
			{
				AST::VariableDeclarator* declarator = global->declarators[i];
				//*instructionStream << "#define " << declarator->variable->mangledName;
				SnekAssert(declarator->value);
				stream << " (" << castValue(declarator->value, global->varType->typeID) << ")";
				newLine();
			}
		}
		else
		{
			if (!HasFlag(global->flags, AST::DeclarationFlags::Constant)) // constants get inlined
			{
				if (HasFlag(global->flags, AST::DeclarationFlags::Extern) && !HasFlag(global->flags, AST::DeclarationFlags::Constant))
					stream << "extern ";

				stream << genType(global->varType->typeID);

				for (int i = 0; i < global->declarators.size; i++)
				{
					AST::VariableDeclarator* declarator = global->declarators[i];
					//*instructionStream << " " << declarator->variable->mangledName;
					if (declarator->value)
						stream << "=" << castValue(declarator->value, global->varType->typeID);
					if (i < global->declarators.size - 1)
						stream << ",";
				}
				stream << ";" << std::endl;
			}
		}

		//globals << globalStream.str();
	}

public:
	CodegenTCC(CGLCompiler* context)
		: context(context)
	{
	}

	AST::Function* getFunction(const char* name)
	{
		for (AST::Function* function : file->functions)
		{
			if (strcmp(function->name, name) == 0)
				return function;
		}
		return nullptr;
	}

	AST::Struct* getStruct(const char* name)
	{
		for (AST::Struct* strct : file->structs)
		{
			if (strcmp(strct->name, name) == 0)
				return strct;
		}
		return nullptr;
	}

	const char* getAttributeName(std::string semantic)
	{
		static std::map<std::string, const char*> semanticNameMap =
		{
			{ "POSITION", "a_position" },
			{ "NORMAL", "a_normal" },
			{ "TANGENT", "a_tangent" },
			{ "BITANGENT", "a_bitangent" },
			{ "COLOR0", "a_color0" },
			{ "COLOR1", "a_color1" },
			{ "COLOR2", "a_color2" },
			{ "COLOR3", "a_color3" },
			{ "BLENDINDICES", "a_indices" },
			{ "BLENDWEIGHT", "a_weight" },
			{ "TEXCOORD0", "a_texcoord0" },
			{ "TEXCOORD1", "a_texcoord1" },
			{ "TEXCOORD2", "a_texcoord2" },
			{ "TEXCOORD3", "i_data4" },
			{ "TEXCOORD4", "i_data3" },
			{ "TEXCOORD5", "i_data2" },
			{ "TEXCOORD6", "i_data1" },
			{ "TEXCOORD7", "i_data0" },
		};
		return semanticNameMap[semantic];
	}

	const char* getVaryingSemantic(int idx)
	{
		static std::vector<const char*> semanticList =
		{
			"TEXCOORD0",
			"TEXCOORD1",
			"TEXCOORD2",
			"NORMAL",
			"TANGENT",
			"BINORMAL",
			"COLOR",

		};
		return semanticList[idx];
	}

	std::string genFile(AST::File* file, bool vertex, std::string* outVaryingDef = nullptr)
	{
		this->file = file;

		stream = std::stringstream();

		const char* vertexFuncName = "vertex";
		const char* fragmentFuncName = "fragment";

		AST::Function* vertexFunc = getFunction(vertexFuncName);
		AST::Function* fragmentFunc = getFunction(fragmentFuncName);
		SnekAssert(vertexFunc->paramTypes.size == 3 && vertexFunc->paramTypes[0]->typeKind == AST::TypeKind::NamedType && vertexFunc->paramTypes[1]->typeKind == AST::TypeKind::NamedType && vertexFunc->paramTypes[2]->typeKind == AST::TypeKind::Vector && ((AST::VectorType*)vertexFunc->paramTypes[2])->size == 4);

		AST::Struct* attributes = getStruct(((AST::NamedType*)vertexFunc->paramTypes[0])->name);
		AST::Struct* varyings = getStruct(((AST::NamedType*)vertexFunc->paramTypes[1])->name);

		stream << "$input ";
		if (vertex)
		{
			for (int i = 0; i < attributes->fields.size; i++)
			{
				stream << getAttributeName(attributes->fields[i]->semantic);
				if (i < attributes->fields.size - 1)
					stream << ", ";
			}
			stream << "\n$output ";

			std::stringstream varyingDef;
			for (int i = 0; i < varyings->fields.size; i++)
			{
				varyingDef << genType(varyings->fields[i]->type) << " __v_" << varyings->fields[i]->name << " : " << getVaryingSemantic(i) << ";\n";
			}
			*outVaryingDef = varyingDef.str();
		}
		for (int i = 0; i < varyings->fields.size; i++)
		{
			stream << "__v_" << varyings->fields[i]->name;
			if (i < varyings->fields.size - 1)
				stream << ", ";
		}
		stream << "\n\n";


		stream <<
#include "includes/bgfx_shader.h"
			;


		for (AST::Struct* strct : file->structs)
		{
			genStruct(strct);
		}
		for (AST::Function* function : file->functions)
		{
			genFunction(function);
		}

		if (vertex)
		{
			stream << "\n\nvoid main() {\n";
			stream << "\t" << attributes->name << " attributes;\n";
			for (int i = 0; i < attributes->fields.size; i++)
			{
				stream << "\tattributes." << attributes->fields[i]->name << " = " << getAttributeName(attributes->fields[i]->semantic) << ";\n";
			}
			stream << "\n\t" << varyings->name << " varyings;\n";
			stream << "\n\t" << vertexFuncName << "(attributes, varyings, gl_Position);\n\n";
			for (int i = 0; i < varyings->fields.size; i++)
			{
				stream << "\t__v_" << varyings->fields[i]->name << " = varyings." << attributes->fields[i]->name << ";\n";
			}
			stream << "}";
		}
		else
		{
			stream << "\n\nvoid main() {\n";
			stream << "\t" << varyings->name << " varyings;\n";
			for (int i = 0; i < varyings->fields.size; i++)
			{
				stream << "\tvaryings." << varyings->fields[i]->name << " = __v_" << attributes->fields[i]->name << ";\n";
			}
			stream << "\n\t" << fragmentFuncName << "(varyings";
			for (int i = 1; i < fragmentFunc->paramTypes.size; i++)
			{
				stream << ", gl_FragData[" << std::to_string(i - 1) << "]";
			}
			stream << ");\n}";
		}

		return stream.str();
	}

	std::string genFileCompute(AST::File* file, const char* kernelName)
	{
		this->file = file;

		stream = std::stringstream();

		AST::Function* kernelFunc = getFunction(kernelName);

		stream <<
#include "includes/bgfx_compute.h"
			;

		for (AST::Struct* strct : file->structs)
		{
			genStruct(strct);
		}
		for (AST::Function* function : file->functions)
		{
			genFunction(function);
		}

		stream << "\n\nvoid main() {\n";
		stream << "\t" << kernelName << "();\n";
		stream << "}";

		return stream.str();
	}
};

void CGLCompiler::output(std::string& vertexSrc, std::string& fragmentSrc, std::string& varyings, bool printIR)
{
	CodegenTCC codegen(this);
	vertexSrc = codegen.genFile(asts[0], true, &varyings);
	fragmentSrc = codegen.genFile(asts[0], false);

	if (printIR)
	{
		printf("%s\n", vertexSrc.c_str());
		printf("%s\n", fragmentSrc.c_str());
	}
}

void CGLCompiler::outputCompute(const char* kernelName, std::string& src, bool printIR)
{
	CodegenTCC codegen(this);
	src = codegen.genFileCompute(asts[0], kernelName);

	if (printIR)
	{
		printf("%s\n", src.c_str());
	}
}
