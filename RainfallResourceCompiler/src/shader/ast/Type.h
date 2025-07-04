#pragma once

#include "Element.h"

#include "../utils/List.h"

#include <stdint.h>


typedef struct TypeData* TypeID;

namespace AST
{
	struct Declaration;
	struct Expression;

	enum class TypeKind : uint8_t
	{
		Null = 0,

		Void,
		Integer,
		FloatingPoint,
		Boolean,
		Any,
		NamedType,
		Struct,
		Class,
		Alias,
		Pointer,
		Optional,
		Function,
		Tuple,
		Array,
		String,
		Vector,
		Matrix,
		Sampler,
	};

	struct Type : Element
	{
		TypeKind typeKind = TypeKind::Null;

		TypeID typeID = nullptr;


		Type(File* file, const SourceLocation& location, TypeKind typeKind);

		virtual Element* copy() override = 0;
	};

	struct VoidType : Type
	{
		VoidType(File* file, const SourceLocation& location);

		virtual Element* copy() override;
	};

	struct IntegerType : Type
	{
		int bitWidth;
		bool isSigned;


		IntegerType(File* file, const SourceLocation& location, int bitWidth, bool isSigned);

		virtual Element* copy() override;
	};

	struct FloatingPointType : Type
	{
		int bitWidth;


		FloatingPointType(File* file, const SourceLocation& location, int bitWidth);

		virtual Element* copy() override;
	};

	struct BooleanType : Type
	{
		BooleanType(File* file, const SourceLocation& location);

		virtual Element* copy() override;
	};

	struct AnyType : Type
	{
		AnyType(File* file, const SourceLocation& location);

		virtual Element* copy() override;
	};

	struct NamedType : Type
	{
		char* name;

		bool hasGenericArgs;
		List<Type*> genericArgs;

		Declaration* declaration = nullptr;


		NamedType(File* file, const SourceLocation& location, char* name, bool hasGenericArgs, const List<Type*>& genericArgs);
		~NamedType();

		virtual Element* copy() override;
	};

	struct PointerType : Type
	{
		Type* elementType;


		PointerType(File* file, const SourceLocation& location, Type* elementType);
		virtual ~PointerType();

		virtual Element* copy() override;
	};

	struct OptionalType : Type
	{
		Type* elementType;


		OptionalType(File* file, const SourceLocation& location, Type* elementType);
		virtual ~OptionalType();

		virtual Element* copy() override;
	};

	struct FunctionType : Type
	{
		Type* returnType;
		List<Type*> paramTypes;
		bool varArgs;
		Type* varArgsType;


		FunctionType(File* file, const SourceLocation& location, Type* returnType, const List<Type*>& paramTypes, bool varArgs, Type* varArgsType);
		virtual ~FunctionType();

		virtual Element* copy() override;
	};

	struct TupleType : Type
	{
		List<Type*> valueTypes;


		TupleType(File* file, const SourceLocation& location, const List<Type*>& valueTypes);
		virtual ~TupleType();

		virtual Element* copy() override;
	};

	struct ArrayType : Type
	{
		Type* elementType;
		Expression* length;


		ArrayType(File* file, const SourceLocation& location, Type* elementType, Expression* length);
		virtual ~ArrayType();

		virtual Element* copy() override;
	};

	struct StringType : Type
	{
		Expression* length;


		StringType(File* file, const SourceLocation& location, Expression* length);

		virtual Element* copy() override;
	};

	struct VectorType : Type
	{
		int size;
		bool integer;

		VectorType(File* file, const SourceLocation& location, int size, bool integer)
			: Type(file, location, AST::TypeKind::Vector), size(size), integer(integer)
		{
		}

		virtual Element* copy() override
		{
			return new VectorType(file, location, size, integer);
		}
	};

	struct MatrixType : Type
	{
		int size;

		MatrixType(File* file, const SourceLocation& location, int size)
			: Type(file, location, AST::TypeKind::Matrix), size(size)
		{
		}

		virtual Element* copy() override
		{
			return new MatrixType(file, location, size);
		}
	};

	struct SamplerType : Type
	{
		bool cubemap;

		SamplerType(File* file, const SourceLocation& location, bool cubemap)
			: Type(file, location, AST::TypeKind::Sampler), cubemap(cubemap)
		{
		}

		virtual Element* copy() override
		{
			return new SamplerType(file, location, cubemap);
		}
	};
}
