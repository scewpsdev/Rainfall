using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;


namespace Rainfall
{
	public class Gradient<T> where T : IMultiplyOperators<T, float, T>, IAdditionOperators<T, T, T>
	{
		struct VectorGradientValue<U> where U : IMultiplyOperators<T, float, T>, IAdditionOperators<T, T, T>
		{
			public U value;
			public float position;

			public VectorGradientValue(float position, U value)
			{
				this.value = value;
				this.position = position;
			}
		}

		List<VectorGradientValue<T>> values = new List<VectorGradientValue<T>>();


		public Gradient(T value)
		{
			clearValues(value);
		}

		public Gradient(T value0, T value1)
		{
			clearValues(value0);
			values.Add(new VectorGradientValue<T>(1.0f, value1));
		}

		public Gradient(Gradient<T> gradient)
		{
			values.AddRange(gradient.values);
		}

		public void clearValues(T value)
		{
			values.Clear();
			values.Add(new VectorGradientValue<T>(0.0f, value));
		}

		int getValueIndex(float position)
		{
			for (int i = 0; i < values.Count; i++)
			{
				if (position < values[i].position)
					return i;
			}
			return values.Count;
		}

		public void setValue(float position, T value)
		{
			for (int i = 0; i < values.Count; i++)
			{
				if (values[i].position == position)
				{
					var newValue = values[i];
					newValue.value = value;
					values[i] = newValue;
					return;
				}
			}

			int index = getValueIndex(position);
			values.Insert(index, new VectorGradientValue<T> { value = value, position = position });
		}

		public T getValue(float position)
		{
			Debug.Assert(values.Count > 0);

			for (int i = 0; i < values.Count; i++)
			{
				VectorGradientValue<T> v1 = values[i];
				if (position < v1.position)
				{
					T value1 = v1.value;
					if (i == 0)
						return value1;
					else
					{
						VectorGradientValue<T> v0 = values[i - 1];
						T value0 = v0.value;
						float progress = (position - v0.position) / (v1.position - v0.position);
						return value0 * (1.0f - progress) + value1 * progress;
					}
				}
			}

			return values[values.Count - 1].value;
		}

		public override bool Equals(object obj)
		{
			return obj is Gradient<T> gradient &&
				   EqualityComparer<List<VectorGradientValue<T>>>.Default.Equals(values, gradient.values);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
