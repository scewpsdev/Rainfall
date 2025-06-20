#pragma once

#include "Application.h"

#include <string.h>
#include <stdlib.h>

#include <bx/allocator.h>


template<typename T, typename U>
struct Pair
{
	T first;
	U second;
};

template<typename T, typename U>
Pair<T, U> CreatePair(const T& t, const U& u)
{
	Pair<T, U> pair;

	pair.first = t;
	pair.second = u;

	return pair;
}

template<typename T>
struct List
{
	T* buffer = nullptr;
	int capacity = 0;
	int size = 0;


	T& operator[](int idx)
	{
		if (idx < this->size)
		{
			return this->buffer[idx];
		}
		else
		{
			__debugbreak();
			return this->buffer[0];
		}
	}

	const T& operator[](int idx) const
	{
		if (idx < this->size)
		{
			return this->buffer[idx];
		}
		else
		{
			__debugbreak();
			return this->buffer[0];
		}
	}

	T& front()
	{
		return (*this)[0];
	}

	T& back()
	{
		return (*this)[size - 1];
	}

	void reserve(int newCapacity)
	{
		T* newBuffer = (T*)BX_ALLOC(Application_GetAllocator(), newCapacity * sizeof(T));
		if (this->buffer)
		{
			int numCopiedElements = __min(newCapacity, this->size);
			memcpy(newBuffer, this->buffer, numCopiedElements * sizeof(T));
			BX_FREE(Application_GetAllocator(), this->buffer);
		}
		this->buffer = newBuffer;
		this->capacity = newCapacity;
		this->size = __min(this->size, newCapacity);
	}

	void resize(int newSize)
	{
		if (newSize > this->capacity)
		{
			reserve(newSize);
		}
		this->size = newSize;
	}

	void add(const T& t)
	{
		while (this->size + 1 > this->capacity)
		{
			reserve(this->capacity + __max(this->capacity / 2, 1));
		}
		this->buffer[this->size++] = t;
	}

	T& add()
	{
		while (this->size + 1 > this->capacity)
		{
			reserve(this->capacity + __max(this->capacity / 2, 1));
		}
		return *(new (&this->buffer[this->size++]) T());
	}

	void addAll(const List<T>& list)
	{
		while (this->size + list.size > this->capacity)
		{
			reserve(this->capacity + __max(this->capacity / 2, 1));
		}
		for (int i = 0; i < list.size; i++)
		{
			add(list[i]);
		}
	}

	void removeAt(int idx)
	{
		if (idx < this->size)
		{
			for (int i = idx; i < this->size - 1; i++)
			{
				this->buffer[i] = this->buffer[i + 1];
			}
			this->size--;
		}
		else
		{
			__debugbreak();
		}
	}

	void insert(int idx, const T& element)
	{
		while (this->size + 1 > this->capacity)
		{
			reserve(this->capacity + __max(this->capacity / 2, 1));
		}
		resize(size + 1);
		for (int i = size - 1; i >= idx + 1; i--)
		{
			buffer[i] = buffer[i - 1];
		}
		buffer[idx] = element;
	}

	int indexOf(const T& t)
	{
		for (int i = 0; i < this->size; i++)
		{
			if (this->buffer[i] == t)
				return i;
		}
		return -1;
	}

	bool contains(const T& t)
	{
		return indexOf(t) != -1;
	}

	void remove(const T& t)
	{
		int idx = indexOf(t);
		if (idx != -1)
			removeAt(idx);
	}

	void clear()
	{
		this->size = 0;
	}

	typedef T* iterator;
	typedef const T* const_iterator;

	iterator begin()
	{
		return buffer ? &buffer[0] : nullptr;
	}

	iterator end()
	{
		return buffer ? &buffer[size] : nullptr;
	}

	const_iterator begin() const
	{
		return buffer ? &buffer[0] : nullptr;
	}

	const_iterator end() const
	{
		return buffer ? &buffer[size] : nullptr;
	}

	List<T> clone() const
	{
		List<T> newList;
		newList.reserve(size);
		newList.addAll(*this);
		return newList;
	}

	void sort(int(*comparator)(const T* a, const T* b))
	{
		qsort(&buffer[0], size, sizeof(T), (_CoreCrtNonSecureSearchSortCompareFunction)comparator);
	}

	void shuffle(uint32_t seed = UINT32_MAX)
	{
		// https://stackoverflow.com/a/6127606

		if (size > 1)
		{
			srand(seed);
			for (int i = 0; i < size - 1; i++)
			{
				int j = i + rand() / (RAND_MAX / (size - i) + 1);
				T& t = buffer[j];
				buffer[j] = buffer[i];
				buffer[i] = t;
			}
		}
	}
};


template<typename T>
List<T> CreateList(int capacity = 0)
{
	List<T> list;

	if (capacity > 0)
		list.reserve(capacity);

	return list;
}

template<typename T>
void DestroyList(List<T>& list)
{
	if (list.buffer)
	{
		BX_FREE(Application_GetAllocator(), list.buffer);
		list.buffer = nullptr;
	}
	list.capacity = 0;
	list.size = 0;
}
