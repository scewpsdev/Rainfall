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
		}

		List<VectorGradientValue<T>> values = new List<VectorGradientValue<T>>();


		public Gradient(T value)
		{
			clearValues(value);
		}

		public Gradient(Gradient<T> gradient)
		{
			values.AddRange(gradient.values);
		}

		public void clearValues(T value)
		{
			values.Clear();
			values.Add(new VectorGradientValue<T> { value = value, position = 0.0f });
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
			int index = getValueIndex(position);
			values.Insert(index, new VectorGradientValue<T> { value = value, position = position });
		}

		public T getValue(float position)
		{
			Debug.Assert(values.Count > 0);

			for (int i = 0; i < values.Count; i++)
			{
				if (position < values[i].position)
				{
					T value1 = values[i].value;
					if (i == 0)
						return value1;
					else
					{
						T value0 = values[i - 1].value;
						float progress = (position - values[i - 1].position) / (values[i].position - values[i - 1].position);
						return value0 * (1.0f - progress) + value1 * progress;
					}
				}
			}

			return values[values.Count - 1].value;
		}
	}
}
