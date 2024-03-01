using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rainfall
{
	public enum DatValueType
	{
		None = 0,
		Number,
		String,
		Identifier,
		Object,
		Array
	}

	public class DatValue
	{
		public readonly DatValueType type;

		public readonly double number;
		public readonly string str;
		public readonly string identifier;
		public readonly DatObject obj;
		public readonly DatArray array;


		public DatValue(double number)
		{
			type = DatValueType.Number;
			this.number = number;
		}

		public DatValue(string str, DatValueType type)
		{
			this.type = type;
			if (type == DatValueType.String)
				this.str = str;
			else if (type == DatValueType.Identifier)
				this.identifier = str;
			else
				Debug.Assert(false);
		}

		public DatValue(DatObject obj)
		{
			type = DatValueType.Object;
			this.obj = obj;
		}

		public DatValue(DatArray array)
		{
			type = DatValueType.Array;
			this.array = array;
		}

		public int integer
		{
			get => (int)number;
		}

		public string stringContent
		{
			get => str.Substring(1, str.Length - 2);
		}
	}

	public class DatField
	{
		public string name = null;
		public DatValue value = null;


		public double number { get => value.number; }
		public string str { get => value.str; }
		public string identifier { get => value.identifier; }
		public DatObject obj { get => value.obj; }
		public DatArray array { get => value.array; }
		public int integer { get => value.integer; }
		public string stringContent { get => value.stringContent; }


		public DatField(string name, DatValue value)
		{
			this.name = name;
			this.value = value;
		}
	}

	public class DatObject
	{
		public List<DatField> fields = new List<DatField>();


		public void addField(DatField field)
		{
			fields.Add(field);
		}

		public void addNumber(string name, double number)
		{
			fields.Add(new DatField(name, new DatValue(number)));
		}

		public void addNumber(string name, float number)
		{
			fields.Add(new DatField(name, new DatValue(number)));
		}

		public void addString(string name, string str)
		{
			fields.Add(new DatField(name, new DatValue(str, DatValueType.String)));
		}

		public void addIdentifier(string name, string identifier)
		{
			fields.Add(new DatField(name, new DatValue(identifier, DatValueType.Identifier)));
		}

		public void addObject(string name, DatObject obj)
		{
			fields.Add(new DatField(name, new DatValue(obj)));
		}

		public void addArray(string name, DatArray arr)
		{
			fields.Add(new DatField(name, new DatValue(arr)));
		}

		public void addVector2(string name, Vector2 v)
		{
			fields.Add(new DatField(name, new DatValue(new DatArray(new DatValue(v.x), new DatValue(v.y)))));
		}

		public void addVector3(string name, Vector3 v)
		{
			fields.Add(new DatField(name, new DatValue(new DatArray(new DatValue(v.x), new DatValue(v.y), new DatValue(v.z)))));
		}

		public void addVector4(string name, Vector4 v)
		{
			fields.Add(new DatField(name, new DatValue(new DatArray(new DatValue(v.x), new DatValue(v.y), new DatValue(v.z), new DatValue(v.w)))));
		}

		public void addQuaternion(string name, Quaternion q)
		{
			fields.Add(new DatField(name, new DatValue(new DatArray(new DatValue(q.x), new DatValue(q.y), new DatValue(q.z), new DatValue(q.w)))));
		}

		public void addInteger(string name, int i)
		{
			fields.Add(new DatField(name, new DatValue(i)));
		}

		public void addBoolean(string name, bool b)
		{
			fields.Add(new DatField(name, new DatValue(b ? 1 : 0)));
		}

		public DatField getField(string name)
		{
			foreach (DatField field in fields)
			{
				if (field.name == name)
					return field;
			}
			return null;
		}

		public bool getField(string name, out DatField field)
		{
			field = getField(name);
			return field != null;
		}

		public bool getNumber(string name, out double number)
		{
			DatField field = getField(name);
			if (field != null && field.value.type == DatValueType.Number)
			{
				number = field.number;
				return true;
			}
			number = 0.0;
			return false;
		}

		public bool getNumber(string name, out float number)
		{
			bool result = getNumber(name, out double d);
			number = (float)d;
			return result;
		}

		public bool getNumber(string name, out int number)
		{
			bool result = getNumber(name, out double d);
			number = (int)Math.Round(d);
			return result;
		}

		public bool getString(string name, out string str)
		{
			DatField field = getField(name);
			if (field != null && field.value.type == DatValueType.String)
			{
				str = field.str;
				return true;
			}
			str = null;
			return false;
		}

		public bool getIdentifier(string name, out string identifier)
		{
			DatField field = getField(name);
			if (field != null && field.value.type == DatValueType.Identifier)
			{
				identifier = field.identifier;
				return true;
			}
			identifier = null;
			return false;
		}

		public bool getObject(string name, out DatObject obj)
		{
			DatField field = getField(name);
			if (field != null && field.value.type == DatValueType.Object)
			{
				obj = field.obj;
				return true;
			}
			obj = null;
			return false;
		}

		public bool getArray(string name, out DatArray arr)
		{
			DatField field = getField(name);
			if (field != null && field.value.type == DatValueType.Array)
			{
				arr = field.array;
				return true;
			}
			arr = null;
			return false;
		}

		public bool getVector2(string name, out Vector2 v)
		{
			DatField field = getField(name);
			if (field != null && field.value.type == DatValueType.Array)
			{
				DatArray arr = field.array;
				Debug.Assert(arr.values.Count == 2);
				v = new Vector2((float)arr.values[0].number, (float)arr.values[1].number);

				return true;
			}
			v = Vector2.Zero;
			return false;
		}

		public bool getVector3(string name, out Vector3 v)
		{
			DatField field = getField(name);
			if (field != null && field.value.type == DatValueType.Array)
			{
				DatArray arr = field.array;
				Debug.Assert(arr.values.Count == 3);
				v = new Vector3((float)arr.values[0].number, (float)arr.values[1].number, (float)arr.values[2].number);

				return true;
			}
			v = Vector3.Zero;
			return false;
		}

		public bool getVector4(string name, out Vector4 v)
		{
			DatField field = getField(name);
			if (field != null && field.value.type == DatValueType.Array)
			{
				DatArray arr = field.array;
				Debug.Assert(arr.values.Count == 4);
				v = new Vector4((float)arr.values[0].number, (float)arr.values[1].number, (float)arr.values[2].number, (float)arr.values[3].number);

				return true;
			}
			v = Vector4.Zero;
			return false;
		}

		public bool getQuaternion(string name, out Quaternion q)
		{
			DatField field = getField(name);
			if (field != null && field.value.type == DatValueType.Array)
			{
				DatArray arr = field.array;
				Debug.Assert(arr.values.Count == 4);
				q = new Quaternion((float)arr.values[0].number, (float)arr.values[1].number, (float)arr.values[2].number, (float)arr.values[3].number);

				return true;
			}
			q = Quaternion.Identity;
			return false;
		}

		public bool getInteger(string name, out int i)
		{
			DatField field = getField(name);
			if (field != null && field.value.type == DatValueType.Number)
			{
				i = field.integer;
				return true;
			}
			i = 0;
			return false;
		}

		public bool getBoolean(string name, out bool b)
		{
			if (getInteger(name, out int i))
			{
				b = i != 0;
				return true;
			}
			b = false;
			return false;
		}

		public bool getStringContent(string name, out string str)
		{
			DatField field = getField(name);
			if (field != null && field.value.type == DatValueType.String)
			{
				str = field.stringContent;
				return true;
			}
			str = null;
			return false;
		}
	}

	public class DatArray
	{
		public List<DatValue> values = new List<DatValue>();


		public DatArray(params DatValue[] values)
		{
			this.values.AddRange(values);
		}

		public int size
		{
			get => values.Count;
		}

		public DatValue this[int index]
		{
			get => values[index];
		}

		public void addNumber(double number)
		{
			values.Add(new DatValue(number));
		}

		public void addNumber(float number)
		{
			values.Add(new DatValue(number));
		}

		public void addString(string str)
		{
			values.Add(new DatValue(str, DatValueType.String));
		}

		public void addIdentifier(string identifier)
		{
			values.Add(new DatValue(identifier, DatValueType.Identifier));
		}

		public void addObject(DatObject obj)
		{
			values.Add(new DatValue(obj));
		}

		public void addArray(DatArray arr)
		{
			values.Add(new DatValue(arr));
		}

		public void addVector2(Vector2 v)
		{
			values.Add(new DatValue(new DatArray(new DatValue(v.x), new DatValue(v.y))));
		}

		public void addVector3(Vector3 v)
		{
			values.Add(new DatValue(new DatArray(new DatValue(v.x), new DatValue(v.y), new DatValue(v.z))));
		}

		public void addVector4(Vector4 v)
		{
			values.Add(new DatValue(new DatArray(new DatValue(v.x), new DatValue(v.y), new DatValue(v.z), new DatValue(v.w))));
		}

		public void addInteger(int i)
		{
			values.Add(new DatValue(i));
		}

		public void addBoolean(bool b)
		{
			values.Add(new DatValue(b ? 1 : 0));
		}
	}

	public class DatFile
	{
		class ParserState
		{
			internal string content;
			internal int idx;

			internal void advance(int num = 1)
			{
				idx += num;
			}

			internal char peek(int offset = 0)
			{
				return idx + offset >= content.Length ? '\0' : content[idx + offset];
			}

			internal char get()
			{
				return content[idx++];
			}

			internal string read(int length)
			{
				string result = content.Substring(idx, length);
				idx += length;
				return result;
			}
		}


		public string path;
		public DatObject root;

		ParserState state;


		public DatFile()
		{
			root = new DatObject();
		}

		public void addField(DatField field)
		{
			root.addField(field);
		}

		public void addNumber(string name, double number)
		{
			root.addNumber(name, number);
		}

		public void addNumber(string name, float number)
		{
			root.addNumber(name, number);
		}

		public void addString(string name, string str)
		{
			root.addString(name, str);
		}

		public void addIdentifier(string name, string identifier)
		{
			root.addIdentifier(name, identifier);
		}

		public void addObject(string name, DatObject obj)
		{
			root.addObject(name, obj);
		}

		public void addArray(string name, DatArray arr)
		{
			root.addArray(name, arr);
		}

		public void addVector2(string name, Vector2 v)
		{
			root.addVector2(name, v);
		}

		public void addVector3(string name, Vector3 v)
		{
			root.addVector3(name, v);
		}

		public void addVector4(string name, Vector4 v)
		{
			root.addVector4(name, v);
		}

		public void addInteger(string name, int i)
		{
			root.addInteger(name, i);
		}

		public void addBoolean(string name, bool b)
		{
			root.addBoolean(name, b);
		}

		void serializeObject(DatObject obj, StreamWriter writer)
		{
			writer.Write("{\n");
			serializeObjectContent(obj, writer);
			writer.Write("\n}");
		}

		void serializeArray(DatArray arr, StreamWriter writer)
		{
			writer.Write("[ ");
			for (int i = 0; i < arr.values.Count; i++)
			{
				DatValue value = arr.values[i];
				serializeValue(value, writer);
				if (i < arr.values.Count - 1)
					writer.Write(", ");
			}
			writer.Write(" ]");
		}

		void serializeValue(DatValue value, StreamWriter writer)
		{
			switch (value.type)
			{
				case DatValueType.Number:
					writer.Write(value.number);
					break;
				case DatValueType.String:
					writer.Write("\"" + value.str + "\"");
					break;
				case DatValueType.Identifier:
					writer.Write(value.identifier);
					break;
				case DatValueType.Object:
					serializeObject(value.obj, writer);
					break;
				case DatValueType.Array:
					serializeArray(value.array, writer);
					break;
			}
		}

		void serializeField(DatField field, StreamWriter writer)
		{
			writer.Write(field.name + " = ");
			serializeValue(field.value, writer);
		}

		void serializeObjectContent(DatObject obj, StreamWriter writer)
		{
			for (int i = 0; i < obj.fields.Count; i++)
			{
				DatField field = obj.fields[i];
				serializeField(field, writer);
				if (i < obj.fields.Count - 1)
					writer.Write("\n");
			}
		}

		public void serialize(Stream stream)
		{
			StreamWriter writer = new StreamWriter(stream);
			serializeObjectContent(root, writer);
			writer.Close();
		}

		public DatFile(string src, string path)
		{
			this.path = path;
			this.root = new DatObject();

			state = new ParserState() { content = src, idx = 0 };
			deserialize(root);
		}

		public DatFile(Stream stream)
		{
			this.root = new DatObject();

			StreamReader reader = new StreamReader(stream);
			string src = reader.ReadToEnd();
			reader.Close();

			state = new ParserState() { content = src, idx = 0 };
			deserialize(root);
		}

		void deserialize(DatObject obj)
		{
			bool hasNext = true;
			while (hasNext)
			{
				skipWhitespaceNewlineComments();

				if (!isAlpha(state.peek()))
					break;

				string name = readIdentifier();

				skipWhitespace();

				char column = state.get();
				Debug.Assert(column == ':' || column == '=');

				skipWhitespace();
				DatValue value = readValue();

				DatField field = new DatField(name, value);
				obj.fields.Add(field);

				skipWhitespaceNewlineComments();
				hasNext = hasNextChar() && state.peek() != '}';
			}
		}

		DatValue readValue()
		{
			skipWhitespace();

			if (nextIsNumber(out int length2))
				return new DatValue(readNumber(length2));
			else if (nextIsString(out int length3))
				return new DatValue(readString(length3), DatValueType.String);
			else if (nextIsIdentifier(out int length4))
				return new DatValue(readIdentifier(length4), DatValueType.Identifier);
			else if (nextIsObject())
				return new DatValue(readObject());
			else if (nextIsArray())
				return new DatValue(readArray());
			else
			{
				Debug.Assert(false);
				return null;
			}
		}

		bool nextIsNumber(out int length)
		{
			if (isDigit(state.peek()) || state.peek() == '-')
			{
				int i = 0;

				while (true)
				{
					char c = state.peek(i);
					if (isDigit(c) || c == '.' || c == '-' && i == 0)
					{
						i++;
					}
					else
					{
						break;
					}
				}

				length = i;
				return true;
			}

			length = 0;
			return false;
		}

		bool nextIsString(out int length)
		{
			if (state.peek() == '"')
			{
				int i = 1;

				while (true)
				{
					char c = state.peek(i);
					if (c != '"')
					{
						i++;
					}
					else
					{
						break;
					}
				}

				i++;

				length = i;
				return true;
			}

			length = 0;
			return false;
		}

		bool nextIsIdentifier(out int length)
		{
			if (isAlpha(state.peek()))
			{
				int i = 0;

				while (true)
				{
					char c = state.peek(i);
					if (isAlpha(c) || isDigit(c) || c == '_' || c == '.' || c == '/' || c == '\\')
					{
						i++;
					}
					else
					{
						break;
					}
				}

				length = i;
				return true;
			}

			length = 0;
			return false;
		}

		bool nextIsObject()
		{
			return state.peek() == '{';
		}

		bool nextIsArray()
		{
			return state.peek() == '[';
		}

		double readNumber(int length)
		{
			return double.Parse(state.read(length), CultureInfo.InvariantCulture);
		}

		string readString(int length)
		{
			return state.read(length);
		}

		string readIdentifier(int length)
		{
			return state.read(length);
		}

		string readIdentifier()
		{
			if (nextIsIdentifier(out int length))
				return readIdentifier(length);
			return null;
		}

		DatObject readObject()
		{
			DatObject obj = new DatObject();

			state.advance(); // {

			skipWhitespaceNewlineComments();
			deserialize(obj);
			skipWhitespaceNewlineComments();

			Debug.Assert(state.peek() == '}');
			state.advance(); // }

			return obj;
		}

		DatArray readArray()
		{
			DatArray array = new DatArray();

			state.advance(); // [
			skipWhitespaceNewlineComments();

			bool hasNext = state.peek() != ']';
			while (hasNext)
			{
				skipWhitespaceNewlineComments();

				DatValue value = readValue();
				array.values.Add(value);

				skipWhitespaceNewlineComments();
				hasNext = state.peek() == ',';
				if (hasNext)
					state.advance(); // ,
			}

			skipWhitespaceNewlineComments();
			Debug.Assert(state.peek() == ']');
			state.advance(); // ]

			return array;
		}

		bool isAlpha(char c)
		{
			return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z';
		}

		bool isDigit(char c)
		{
			return c >= '0' && c <= '9';
		}

		bool isWhitespace(char c)
		{
			return c == ' ' || c == '\t';
		}

		bool isNewline(char c)
		{
			return c == '\n' || c == '\r';
		}

		bool hasNextChar(int count = 1)
		{
			//skipWhitespaceAndNewline();
			for (int i = 0; i < count; i++)
			{
				if (!(state.peek(i) != '\0' && state.peek(i) != -51))
					return false;
			}
			return true;
		}

		void skipWhitespace()
		{
			while (isWhitespace(state.peek()))
				state.advance();
		}

		void skipWhitespaceNewlineComments()
		{
			while (true)
			{
				if (hasNextChar() && (isWhitespace(state.peek()) || isNewline(state.peek())))
				{
					state.advance();
				}
				else if (hasNextChar(2) && state.peek() == '/' && state.peek(1) == '/')
				{
					state.advance(); // /
					state.advance(); // /
					while (hasNextChar() && !isNewline(state.peek()))
					{
						state.advance();
					}
				}
				else
				{
					break;
				}
			}
		}

		public DatField getField(string name)
		{
			return root.getField(name);
		}

		public bool getField(string name, out DatField field)
		{
			return root.getField(name, out field);
		}

		public bool getNumber(string name, out double number)
		{
			return root.getNumber(name, out number);
		}

		public bool getNumber(string name, out float number)
		{
			return root.getNumber(name, out number);
		}

		public bool getString(string name, out string str)
		{
			return root.getString(name, out str);
		}

		public bool getIdentifier(string name, out string identifier)
		{
			return root.getIdentifier(name, out identifier);
		}

		public bool getObject(string name, out DatObject obj)
		{
			return root.getObject(name, out obj);
		}

		public bool getArray(string name, out DatArray arr)
		{
			return root.getArray(name, out arr);
		}

		public bool getVector2(string name, out Vector2 v)
		{
			return root.getVector2(name, out v);
		}

		public bool getVector3(string name, out Vector3 v)
		{
			return root.getVector3(name, out v);
		}

		public bool getVector4(string name, out Vector4 v)
		{
			return root.getVector4(name, out v);
		}

		public bool getInteger(string name, out int i)
		{
			return root.getInteger(name, out i);
		}

		public bool getBoolean(string name, out bool b)
		{
			return root.getBoolean(name, out b);
		}

		public bool getStringContent(string name, out string str)
		{
			return root.getStringContent(name, out str);
		}
	}
}
