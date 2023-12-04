using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}

	public class DatObject
	{
		public List<DatField> fields = new List<DatField>();

		public DatField getField(string name)
		{
			foreach (DatField field in fields)
			{
				if (field.name == name)
					return field;
			}
			return null;
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

		public bool getObject(string name, out DatArray arr)
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


		public DatFile(string src, string path)
		{
			this.path = path;
			this.root = new DatObject();

			state = new ParserState() { content = src, idx = 0 };
			readObjectContent(root);
		}

		void readObjectContent(DatObject obj)
		{
			bool hasNext = true;
			while (hasNext)
			{
				skipWhitespaceNewlineComments();

				if (!isAlpha(state.peek()))
					break;

				DatField field = new DatField();
				field.name = readIdentifier();

				skipWhitespace();

				char column = state.get();
				Debug.Assert(column == ':' || column == '=');

				skipWhitespace();
				field.value = readValue();

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
			readObjectContent(obj);
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

			bool hasNext = true;
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

		public bool getObject(string name, out DatArray arr)
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
}
