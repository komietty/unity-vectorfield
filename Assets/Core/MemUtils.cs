using System;
using System.Buffers;

namespace ddg {
    ref struct TempList<T> {
        int index;
        T[] array;
        public ReadOnlySpan<T> Span => new ReadOnlySpan<T>(array, 0, index); 
        public TempList(int initCapacity) {
            this.array = ArrayPool<T>.Shared.Rent(initCapacity);
            this.index = 0;
        }

        public void Add(T value) {
            if(array.Length <= index) {
                var newArr = ArrayPool<T>.Shared.Rent(index * 2);
                Array.Copy(array, newArr, index);
                ArrayPool<T>.Shared.Return(array, true);
                array = newArr;
            }
            array[index++] = value;
        }

        public void Dispose() {
            ArrayPool<T>.Shared.Return(array, true);
        }
    }
}