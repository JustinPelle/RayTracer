using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RayTracer_JP
{

	// A base-like class for the GL-texture used to draw our pixels to.
	// Facilitates properties and methods for both the debug view and main camera-view.
	public class GLTexture2D
    {
		public int Id;
		public int Width, Height;

		public int[] DebugPixels;
		public int[] CamPixels;


		public GLTexture2D(int w, int h) 
		{
			this.Width = w;
			this.Height = h;
			this.DebugPixels = new int[w * h];
			this.CamPixels = new int[w * h];
			this.Id = this.generateGLTexture();
		}


		// Creates an OpenGL texture.
		private int generateGLTexture()
		{
			// Create, bind and set parameters for the texture.
			int id = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, id);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

			// Bind and set the specifics of the pixels to-be-drawn
			GL.TexImage2D(
				TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 
				this.Width, this.Height, 0,
				PixelFormat.Bgra, PixelType.UnsignedByte, this.DebugPixels
			);
			GL.TexImage2D(
				TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
			   this.Width, this.Height, 0, PixelFormat.Bgra,
			   PixelType.UnsignedByte, this.CamPixels
			);

			return id;
		}


		// Focuses the viewport of the to-be-drawn debug pixels.
		public void DebugPixelsToTexImage2D()
		{
			GL.Viewport(0, 0, this.Width, this.Height);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
			   this.Width, this.Height, 0, PixelFormat.Bgra,
			   PixelType.UnsignedByte, this.DebugPixels
			);
		}

		// Focuses the viewport of the to-be-drawn camera pixels.
		public void CamPixelsToTextImage2D()
        {
			GL.Viewport(this.Width, 0, this.Width, this.Height);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
			   this.Width, this.Height, 0, PixelFormat.Bgra,
			   PixelType.UnsignedByte, this.CamPixels
			);
		}

		// Quad-fill the texture drawing.
		public void DrawQuadFill()
		{
			GL.Begin(PrimitiveType.Quads);
			GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1.0f, -1.0f);
			GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1.0f, -1.0f);
			GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1.0f, 1.0f);
			GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1.0f, 1.0f);
			GL.End();
		}
	}



	// Instantiating class of the DisplayFrame (part of the application window to-be-drawn to).
	// Facilitates properties and methods for both the debug view and main camera-view.
	public class DisplayFrame : GLTexture2D
	{

		public int DebugBgColor;
		public Vector3 DebugPrimaryRayColor = Vec3Color.GrayScale(0.7f),
			DebugSecondaryRayColor = Vec3Color.GrayScale(0.7f),
			DebugShadowRayColor = Vec3Color.GrayScale(0.4f);

		private static float debugXmin = -5f,
			debugXmax = 5f,
			debugYmin = 0f,
			debugYmax = 10f;


		public DisplayFrame(int w, int h, Vector3 debugBgColor) : base(w, h) 
		{
			this.DebugBgColor = Vec3Color.ToIntRGB(debugBgColor);
		}


		// Converts an x-coordinate to the 'column' based pixel-position J of the debug-view.
		private int toDebugPositionJ(float x)
			=> (int)(this.Width * (x - debugXmin) / (debugXmax - debugXmin));

		// Converts an y-coordinate to the 'row' based pixel-position I of the debug-view.
		// N.B. row-order position I is reversed w.r.t y-coordinate.
		private int toDebugPositionI(float y)
			=> this.Height - (int)(this.Height * (y - debugYmin) / (debugYmax - debugYmin));



		// Clears the debug view by setting the pixels to a uniform color.
		public void ClearDebugPixels(int c)
		{
			for (int s = this.Width * this.Height, p = 0; p < s; p++) this.DebugPixels[p] = c;
		}

		// Clears the camera view by setting the pixels to a uniform color.
		public void ClearCamPixels(int c)
		{
			for (int s = this.Width * this.Height, p = 0; p < s; p++) this.CamPixels[p] = c;
		}

		// Puts a color on certain position i,j of the debug screen
		public void PutDebugPixel(int i, int j, int color)
			=> this.DebugPixels[j + (i * this.Width)] = color;

		// Puts a color on certain position i,j of the camera screen
		public void PutCamPixel(int i, int j, int color)
			=> this.CamPixels[j + (i * this.Width)] = color;


		// Draws a sphere to the debug screen as a circle (only if within the debug-view bounds).
		public void DrawDebugSphere(Vector3 pos, float r, Vector3 color)
		{
			int c = Vec3Color.ToIntRGB(color), i, j;
			float a, x = pos.X, y = pos.Y;

			// Early exit if sphere x- and y- bounds not within display bounds.
			if (!(this.toDebugPositionI(y - r) >= 0)
				|| !(this.toDebugPositionI(y + r) < this.Height)
				|| !(this.toDebugPositionJ(x-r) >= 0)
				|| !(this.toDebugPositionJ(x+r) < this.Width))
				return;

			// Draw the x,y points of the circle by the angle increment of 1 degrees up until 360.
			for (a = 0.0f; a < 2*Math.PI; a += (float)(Math.PI / 180f))
			{
				x = (float)(pos.X + r * Math.Cos(a));
				y = (float)(pos.Y + r * Math.Sin(a));
				i = this.toDebugPositionI(y);
				j = this.toDebugPositionJ(x);
				this.PutDebugPixel(i,j, c);
			}
		}

		// Draws the x,y points of a line to the debug screen by differentially incrementing steps.
		public void DrawDebugLine(Vector3 p1, Vector3 p2, Vector3 color)
		{
			// calculate i,j positions on debug-screen of the two line points, and coefficients.s
			int c = Vec3Color.ToIntRGB(color);
			int x1 = this.toDebugPositionJ(p1.X), 
				x2 = this.toDebugPositionJ(p2.X),
				y1 = this.toDebugPositionI(p1.Y), 
				y2 = this.toDebugPositionI(p2.Y);
			float dx = x2 - x1,
				dy = y2 - y1,
				coeff = (Math.Abs(dx) >= Math.Abs(dy)) ? Math.Abs(dx) : Math.Abs(dy),
				i = y1,
				j = x1;
			dx /= coeff; 
			dy /= coeff;

			// Draw the x,y points of the line by incrementing the i and j position relative to coefficient(s).
			for (int k = 1; k <= coeff; k++) {
				if (i < 0 | i >= this.Height | j >= this.Width | j < 0)
					break;
				this.PutDebugPixel((int)i, (int)j, c);
				i += dy;
				j += dx; 
			}
		}

	}

}
