#pragma once

#include "../Common.h"
#include "Element.h"
#include "Type.h"
#include "Statement.h"
#include "Expression.h"

#include "../utils/List.h"
#include "../semantics/Type.h"

#include <stdint.h>


struct Variable;

namespace AST
{
	enum class DeclarationType : uint8_t
	{
		Null = 0,

		Function,
		Struct,
		Class,
		ClassMethod,
		ClassConstructor,
		Typedef,
		Enumeration,
		Macro,
		GlobalVariable,
		Module,
		Namespace,
		Import,
	};

	enum class DeclarationFlags : uint16_t
	{
		None = 0,
		Constant = 1 << 0,
		Extern = 1 << 1,
		DllExport = 1 << 2,
		DllImport = 1 << 3,
		Public = 1 << 4,
		Private = 1 << 5,
		Internal = 1 << 6,
		Packed = 1 << 7,
	};

	DeclarationFlags operator|(DeclarationFlags flag0, DeclarationFlags flag1);
	DeclarationFlags operator&(DeclarationFlags flag0, DeclarationFlags flag1);

	enum class OperatorOverload : uint8_t
	{
		None = 0,
		Subscript,
	};

	struct Declaration : Element
	{
		DeclarationType type;
		DeclarationFlags flags;

		Visibility visibility = Visibility::Null;


		Declaration(File* file, const SourceLocation& location, DeclarationType type, DeclarationFlags flags);

		virtual Element* copy() override = 0;
	};

	struct Function : Declaration
	{
		char* name = nullptr;
		Type* returnType = nullptr;
		List<Type*> paramTypes;
		List<char*> paramNames;
		List<int> paramFlags;

		Statement* body = nullptr;
		Expression* bodyExpression = nullptr;

		OperatorOverload operatorOverload = OperatorOverload::None;

		bool isGeneric = false;
		bool isGenericInstance = false;
		List<char*> genericParams;
		List<TypeID> genericTypeArguments;
		List<Function*> genericInstances;

		SourceLocation endLocation;

		bool isEntryPoint = false;
		char* mangledName = nullptr;
		TypeID functionType = nullptr;

		//List<Variable*> paramVariables = {};
		Variable* instanceVariable = nullptr;

		ValueHandle valueHandle = nullptr;


		Function(File* file, const SourceLocation& location, DeclarationFlags flags, const SourceLocation& endLocation, char* name, Type* returnType, const List<Type*>& paramTypes, const List<char*>& paramNames, Statement* body, Expression* bodyExpression, bool isGeneric, const List<char*>& genericParams);
		Function(File* file, const SourceLocation& location, DeclarationFlags flags, const SourceLocation& endLocation, char* name, Type* returnType, const List<Type*>& paramTypes, const List<char*>& paramNames, const List<int>& paramFlags, Statement* body, bool isGeneric, const List<char*>& genericParams);
		Function(File* file, const SourceLocation& location, DeclarationFlags flags, const SourceLocation& endLocation, char* name, Type* returnType, const List<Type*>& paramTypes, const List<char*>& paramNames, Expression* bodyExpression, bool isGeneric, const List<char*>& genericParams);
		virtual ~Function();

		virtual Element* copy() override;

		int getNumRequiredParams();

		bool isGenericParam(int idx, TypeID argType) const;
		TypeID getGenericTypeArgument(const char* name);
		Function* getGenericInstance(const List<TypeID>& genericArgs);
	};

	struct Method : Function
	{
		TypeID instanceType = nullptr; // For class methods/constructors


		Method(File* file, const SourceLocation& location, DeclarationFlags flags, const SourceLocation& endLocation, char* name, Type* returnType, const List<Type*>& paramTypes, const List<char*>& paramNames, Statement* body, bool isGeneric, const List<char*>& genericParams);
		virtual ~Method();

		virtual Element* copy() override;
	};

	struct Constructor : Method
	{
		Constructor(File* file, const SourceLocation& location, DeclarationFlags flags, const SourceLocation& endLocation, char* name, Type* returnType, const List<Type*>& paramTypes, const List<char*>& paramNames, Statement* body, bool isGeneric, const List<char*>& genericParams);
		virtual ~Constructor();

		virtual Element* copy() override;
	};

	struct StructField : Element
	{
		Type* type;
		char* name;
		int index;
		char* semantic;


		StructField(File* file, const SourceLocation& location, Type* type, char* name, int index, char* semantic);
		virtual ~StructField();

		virtual Element* copy() override;
	};

	struct Struct : Declaration
	{
		char* name;
		bool hasBody;
		List<StructField*> fields;

		bool isGeneric = false;
		bool isGenericInstance = false;
		List<char*> genericParams;
		List<TypeID> genericTypeArguments;

		List<Struct*> genericInstances;

		char* mangledName = nullptr;
		TypeID type = nullptr;

		TypeHandle typeHandle = nullptr;


		Struct(File* file, const SourceLocation& location, DeclarationFlags flags, char* name, bool hasBody, const List<StructField*>& fields, bool isGeneric, const List<char*>& genericParams);
		virtual ~Struct();

		virtual Element* copy() override;

		TypeID getGenericTypeArgument(const char* name);
		Struct* getGenericInstance(const List<Type*>& genericArgs);
	};

	struct ClassField : Element
	{
		Type* type;
		char* name;
		int index;


		ClassField(File* file, const SourceLocation& location, Type* type, char* name, int index);
		virtual ~ClassField();

		virtual Element* copy() override;
	};

	struct Class : Declaration
	{
		char* name;
		List<ClassField*> fields;
		List<Method*> methods;
		Constructor* constructor;

		bool isGeneric = false;
		bool isGenericInstance = false;
		List<char*> genericParams;
		List<TypeID> genericTypeArguments;

		List<Class*> genericInstances;

		char* mangledName = nullptr;
		TypeID type = nullptr;

		TypeHandle typeHandle = nullptr;


		Class(File* file, const SourceLocation& location, DeclarationFlags flags, char* name, const List<ClassField*>& fields, const List<Method*>& methods, Constructor* constructor, bool isGeneric, const List<char*>& genericParams);
		virtual ~Class();

		virtual Element* copy() override;

		TypeID getGenericTypeArgument(const char* name);
		Class* getGenericInstance(const List<Type*>& genericArgs);
	};

	struct Typedef : Declaration
	{
		char* name;
		Type* alias;

		TypeID type = nullptr;


		Typedef(File* file, const SourceLocation& location, DeclarationFlags flags, char* name, Type* alias);
		virtual ~Typedef();

		virtual Element* copy() override;
	};

	struct Enum;

	struct EnumValue : Element
	{
		char* name;
		Expression* value;

		Enum* declaration = nullptr;

		ValueHandle valueHandle = nullptr;


		EnumValue(File* file, const SourceLocation& location, char* name, Expression* value);
		virtual ~EnumValue();

		virtual Element* copy() override;
	};

	struct Enum : Declaration
	{
		char* name;
		Type* alias;
		List<EnumValue*> values;

		TypeID type = nullptr;


		Enum(File* file, const SourceLocation& location, DeclarationFlags flags, char* name, Type* alias, const List<EnumValue*>& values);
		virtual ~Enum();

		virtual Element* copy() override;
	};

	struct Macro : Declaration
	{
		char* name;
		Expression* alias;


		Macro(File* file, const SourceLocation& location, DeclarationFlags flags, char* name, Expression* alias);
		virtual ~Macro();

		virtual Element* copy() override;
	};

	struct GlobalVariable : Declaration
	{
		Type* varType;
		List<VariableDeclarator*> declarators;


		GlobalVariable(File* file, const SourceLocation& location, DeclarationFlags flags, Type* varType, List<VariableDeclarator*>& declarators);
		virtual ~GlobalVariable();

		virtual Element* copy() override;
	};

	struct ModuleIdentifier
	{
		List<char*> namespaces;
	};

	struct ModuleDeclaration : Declaration
	{
		ModuleIdentifier identifier;

		struct Module* module = nullptr;


		ModuleDeclaration(File* file, const SourceLocation& location, DeclarationFlags flags, ModuleIdentifier identifier);
		virtual ~ModuleDeclaration();

		virtual Element* copy() override;
	};

	struct NamespaceDeclaration : Declaration
	{
		char* name;


		NamespaceDeclaration(File* file, const SourceLocation& location, DeclarationFlags flags, char* name);
		virtual ~NamespaceDeclaration();

		virtual Element* copy() override;
	};

	struct Import : Declaration
	{
		List<ModuleIdentifier> imports;


		Import(File* file, const SourceLocation& location, DeclarationFlags flags, const List<ModuleIdentifier>& imports);
		virtual ~Import();

		virtual Element* copy() override;
	};
}
