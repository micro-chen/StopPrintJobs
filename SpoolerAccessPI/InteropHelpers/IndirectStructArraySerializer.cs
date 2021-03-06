// Released into the public domain.
// http://creativecommons.org/publicdomain/zero/1.0/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SpoolerAccessPI.InteropHelpers
{
    /// <summary>
    /// Serializes multiple structs into memory (not necessarily contiguously) and provides a pointer
    /// to an array of pointers at which the structs are stored.
    /// </summary>
    internal class IndirectStructArraySerializer<T> : IDisposable
    {
        /// <summary>
        /// Contains the structs that will be serialized when <see cref="Serialize()"/> is called and deserialized
        /// when <see cref="Deserialize()"/> is called.
        /// </summary>
        public List<T> TheStructs { get; private set; }

        /// <summary>
        /// Once Serialize() is called, contains a pointer to an array of pointers to serialized versions of the structs.
        /// </summary>
        public IntPtr PointerToStructPointers { get; private set; }

        /// <summary>
        /// Once Serialize() is called, contains the size of one serialized struct.
        /// </summary>
        public int? StructSize { get; private set; }

        /// <summary>
        /// Once Serialize() is called, contains the number of structs that have been serialized.
        /// </summary>
        public int? SerializedCount { get; private set; }

        private List<IntPtr> StructPointers { get; set; }
        private bool IsDisposed { get; set; }

        /// <summary>
        /// Creates a new serializer for the given array of structs.
        /// </summary>
        /// <param name="theStructs">The structs for which to create the serializer.</param>
        public IndirectStructArraySerializer(IEnumerable<T> theStructs)
        {
            TheStructs = new List<T>(theStructs);
            PointerToStructPointers = IntPtr.Zero;
            StructSize = null;
            SerializedCount = null;

            StructPointers = new List<IntPtr>();
            IsDisposed = false;
        }

        /// <summary>
        /// Creates a new serializer for an initially empty array of structs.
        /// </summary>
        public IndirectStructArraySerializer() : this(new T[] {})
        {
        }

        /// <summary>
        /// Serializes the structs, making PointerToStructPointers point to the area in memory where the
        /// array of pointers to the structs is stored, setting StructSize to the size of one serialized struct,
        /// and setting SerializedCount to the number of structs that have been serialized.
        /// </summary>
        public void Serialize()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            // deallocate the previous pointers
            DeallocateSerializedStructs();

            StructSize = Marshal.SizeOf(typeof(T));

            // marshal the structs individually
            foreach (var theStruct in TheStructs)
            {
                var structPointer = Marshal.AllocHGlobal(StructSize.Value);
                StructPointers.Add(structPointer);
                Marshal.StructureToPtr(theStruct, structPointer, false);
            }

            // assemble the array of pointers to the structs
            PointerToStructPointers = Marshal.AllocHGlobal(StructPointers.Count * IntPtr.Size);
            Marshal.Copy(StructPointers.ToArray(), 0, PointerToStructPointers, StructPointers.Count);

            // store SerializedCount
            SerializedCount = TheStructs.Count;
        }

        /// <summary>
        /// Deserializes a different struct array, loading the structs into TheStructs. Other public-facing
        /// properties remain untouched.
        /// </summary>
        public void Deserialize(int count, IntPtr pointerToStructPointers)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            var localStructPointers = new IntPtr[count];
            Marshal.Copy(pointerToStructPointers, localStructPointers, 0, count);

            TheStructs.Clear();
            foreach (var structPointer in localStructPointers)
            {
                TheStructs.Add((T)Marshal.PtrToStructure(structPointer, typeof(T)));
            }
        }

        /// <summary>
        /// Deserializes the last serialized struct array, loading the structs back into TheStructs.
        /// </summary>
        public void Deserialize()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            // clear the old list
            TheStructs.Clear();

            foreach (var structPointer in StructPointers)
            {
                TheStructs.Add((T)Marshal.PtrToStructure(structPointer, typeof(T)));
            }
        }

        protected void DeallocateSerializedStructs()
        {
            if (PointerToStructPointers != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(PointerToStructPointers);
                PointerToStructPointers = IntPtr.Zero;
            }
            foreach (var structPointer in StructPointers)
            {
                Marshal.DestroyStructure(structPointer, typeof(T));
                Marshal.FreeHGlobal(structPointer);
            }
            StructPointers.Clear();
        }

        /// <summary>
        /// Releases all resource used by the <see cref="SpoolerAccessPI.StructSerializer`1"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="SpoolerAccessPI.StructSerializer`1"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="SpoolerAccessPI.StructSerializer`1"/> in an unusable state.
        /// After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="SpoolerAccessPI.StructSerializer`1"/> so the garbage collector can reclaim the memory that the
        /// <see cref="SpoolerAccessPI.StructSerializer`1"/> was occupying.</remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                // if (disposing) { /* clean up managed resources */ }

                // clean up all the allocated stuff
                DeallocateSerializedStructs();
            }
            IsDisposed = true;
        }

        ~IndirectStructArraySerializer()
        {
            Dispose(false);
        }
    }
}
