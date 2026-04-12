using System;
using System.Collections.Generic;
using System.Text;

namespace Flat2.Core.Maths
{
    using Silk.NET.Maths;
    using System.Runtime.CompilerServices;

    namespace YourProject.Extensions
    {
        public static class VectorMappingExtensions
        {
            // --- 从 System.Numerics.Vector2 转换 ---

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Silk.NET.Maths.Vector2D<float> ToSilk(this System.Numerics.Vector2 v)
                => new(v.X, v.Y);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static SkiaSharp.SKPoint ToSkia(this System.Numerics.Vector2 v)
                => new(v.X, v.Y);


            // --- 从 Silk.NET.Maths.Vector2D<float> 转换 ---

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static System.Numerics.Vector2 ToSystem(this Silk.NET.Maths.Vector2D<float> v)
                => new(v.X, v.Y);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static SkiaSharp.SKPoint ToSkia(this Silk.NET.Maths.Vector2D<float> v)
                => new(v.X, v.Y);


            // --- 从 SkiaSharp.SKPoint (Vector2) 转换 ---

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static System.Numerics.Vector2 ToSystem(this SkiaSharp.SKPoint v)
                => new(v.X, v.Y);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Silk.NET.Maths.Vector2D<float> ToSilk(this SkiaSharp.SKPoint v)
                => new(v.X, v.Y);
        }
    }
}
