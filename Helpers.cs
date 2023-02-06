using System;
using OpenTK;

namespace RayTracer_JP
{

	// Static supportive class encapsulating math-functions.
	public static class MathHelpers
	{

		// Limits a float value between min and max.
		public static float FloatLimitBetween(float v, float min, float max)
			=> Math.Max(min, Math.Min(v, max));

		// Converts polar coordinates to Vector3 carthesian coordinates.
		public static Vector3 PolarToCarth(float r, float theta, float phi)
		{
			float x = r * (float)(Math.Sin(theta) * Math.Sin(phi)),
				y = r * (float)(Math.Sin(theta) * Math.Cos(phi)),
				z = r * (float)Math.Cos(theta);
			return new Vector3(x, y, z);
		}

		// Converts carthesian coordinates to polar theta angle.
		public static float CarthToPolarTheta(Vector3 p)
			=> (float)Math.Acos(p.Z / p.Length);

		// Converts carthesian coordinates to polar phi angle.
		public static float CarthToPolarPhi(Vector3 p)
			=> (p.Y == 0f && p.X == 0f) ? 0f : (float)Math.Acos(p.Y / Math.Sqrt(p.Y * p.Y + p.X * p.X));

	}

	
	// Static supportive class encapsulating pre-defined colors as Vector3
	// and conversion from- and to- 32-bit integer RGB values.
	public static class Vec3Color
	{

		public static Vector3 White = new Vector3(1f, 1f, 1f);
		public static Vector3 Black = new Vector3(0f, 0f, 0f);
		public static Vector3 Red = new Vector3(1f, 0f, 0f);
		public static Vector3 Green = new Vector3(0f, 1f, 0f);
		public static Vector3 Blue = new Vector3(0f, 0f, 1f);
		public static Vector3 Yellow = new Vector3(1f, 1f, 0f);
		public static Vector3 Purple = new Vector3(1f, 0f, 1f);
		public static Vector3 Cyan = new Vector3(0f, 1f, 1f);
		public static Vector3 Orange = new Vector3(1f, 0.5f, 0f);


		// Creates a color given a gray-scale value 0<val<1.
		public static Vector3 GrayScale(float gray)
			=> new Vector3(gray, gray, gray);

		// Converts a Vector3 color to a 32-bit integer RGB value.
		public static int ToIntRGB(Vector3 color)
			=> (int)(color.X * 0xFF) << 16 
				| (int)(color.Y * 0xFF) << 8 
				| (int)(color.Z * 0xFF);

		// Converts a 32-bit integer RGB color to Vector3 object.
		public static Vector3 FromIntRGB(int c)
			=> new Vector3(
				(0xFF & (c >> 16))/(float)0xFF, 
				(0xFF & (c >> 8))/(float)0xFF, 
				(0xFF & c)/(float)0xFF
			);

	}

}
