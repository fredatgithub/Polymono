using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.IO;

namespace Polymono.Graphics {
    abstract class AModel {
        // Identification
        public static int TOTAL_IDS = 0;
        public int ID;
        // Buffer references
        public int VBO;
        public int VAO;
        public int IBO;
        public int TextureID;
        // Texture data
        public float[] TexData;
        public int Width, Height, OffWidth, OffHeight;
        // Matrices
        public Matrix4 ModelMatrix;
        // Spatial data
        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Scaling = Vector3.One;

        public AModel() : this(Vector3.Zero, Vector3.Zero, Vector3.One)
        {
            
        }

        public AModel(Vector3 position, Vector3 rotation, Vector3 scaling)
        {
            ID = TOTAL_IDS++;
            Position = position;
            Rotation = rotation;
            Scaling = scaling;
        }

        public abstract void CreateBuffer();

        public virtual void Render()
        {
            Polymono.Debug($"Render methods don't exist for ID: {ID}");
        }

        public virtual void RenderObject(ProgramID id)
        {
            Render();
        }

        public abstract void Delete();

        protected int LoadTexture(string filename, int offWidth = 0, int offHeight = 0)
        {
            OffWidth = offWidth;
            OffHeight = offHeight;
            try
            {
                using (var bmp = (Bitmap)Image.FromFile(filename))
                {
                    Width = bmp.Width;
                    Height = bmp.Height;
                    TexData = new float[Width * Height * 4];
                    int index = 0;
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            var pixel = bmp.GetPixel(x, y);
                            TexData[index++] = pixel.R / 255f;
                            TexData[index++] = pixel.G / 255f;
                            TexData[index++] = pixel.B / 255f;
                            TexData[index++] = pixel.A / 255f;
                        }
                    }
                }
            } catch (FileNotFoundException e)
            {
                Polymono.Debug($"Cannot find {filename}: {e}");
                return -1;
            }
            GL.CreateTextures(TextureTarget.Texture2D, 1, out int texture);
            GL.TextureStorage2D(texture, 1, SizedInternalFormat.Rgba32f, Width, Height);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TextureSubImage2D(texture, 0, OffWidth, OffHeight, Width, Height, PixelFormat.Rgba,
                PixelType.Float, TexData);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }

        public int CreateTexture(string path, int quality = 1, bool repeat = false, bool flip_y = false)
        {
            Bitmap bitmap;
            try
            {
                bitmap = new Bitmap(path);
            } catch (FileNotFoundException e)
            {
                Polymono.Debug($"Cannot find {path}: {e}");
                return -1;
            }

            //Flip the image
            if (flip_y)
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            //Generate a new texture target in gl
            int texture = GL.GenTexture();

            //Will bind the texture newly/empty created with GL.GenTexture
            //All gl texture methods targeting Texture2D will relate to this texture
            GL.BindTexture(TextureTarget.Texture2D, texture);

            //The reason why your texture will show up glColor without setting these parameters is actually
            //TextureMinFilters fault as its default is NearestMipmapLinear but we have not established mipmapping
            //We are only using one texture at the moment since mipmapping is a collection of textures pre filtered
            //I'm assuming it stops after not having a collection to check.
            switch (quality)
            {
                case 0:
                default://Low quality
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
                    break;
                case 1://High quality
                       //This is in my opinion the best since it doesnt average the result and not blurred to shit
                       //but most consider this low quality...
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
                    break;
            }

            if (repeat)
            {
                //This will repeat the texture past its bounds set by TexImage2D
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);
            } else
            {
                //This will clamp the texture to the edge, so manipulation will result in skewing
                //It can also be useful for getting rid of repeating texture bits at the borders
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            }

            //Creates a definition of a texture object in opengl
            /* Parameters
             * Target - Since we are using a 2D image we specify the target Texture2D
             * MipMap Count / LOD - 0 as we are not using mipmapping at the moment
             * InternalFormat - The format of the gl texture, Rgba is a base format it works all around
             * Width;
             * Height;
             * Border - must be 0;
             * 
             * Format - this is the images format not gl's the format Bgra i believe is only language specific
             *          C# uses little-endian so you have ARGB on the image A 24 R 16 G 8 B, B is the lowest
             *          So it gets counted first, as with a language like Java it would be PixelFormat.Rgba
             *          since Java is big-endian default meaning A is counted first.
             *          but i could be wrong here it could be cpu specific :P
             *          
             * PixelType - The type we are using, eh in short UnsignedByte will just fill each 8 bit till the pixelformat is full
             *             (don't quote me on that...)
             *             you can be more specific and say for are RGBA to little-endian BGRA -> PixelType.UnsignedInt8888Reversed
             *             this will mimic are 32bit uint in little-endian.
             *             
             * Data - No data at the moment it will be written with TexSubImage2D
             */
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            //Load the data from are loaded image into virtual memory so it can be read at runtime
            System.Drawing.Imaging.BitmapData bitmap_data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            //Writes data to are texture target
            /* Target;
             * MipMap;
             * X Offset - Offset of the data on the x axis
             * Y Offset - Offset of the data on the y axis
             * Width;
             * Height;
             * Format;
             * Type;
             * Data - Now we have data from the loaded bitmap image we can load it into are texture data
             */
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, bitmap.Width, bitmap.Height, PixelFormat.Bgra, PixelType.UnsignedByte, bitmap_data.Scan0);

            //Release from memory
            bitmap.UnlockBits(bitmap_data);

            //get rid of bitmap object its no longer needed in this method
            bitmap.Dispose();

            /*Binding to 0 is telling gl to use the default or null texture target
            *This is useful to remember as you may forget that a texture is targeted
            *And may overflow to functions that you dont necessarily want to
            *Say you bind a texture
            *
            * Bind(Texture);
            * DrawObject1();
            *                <-- Insert Bind(NewTexture) or Bind(0)
            * DrawObject2();
            * 
            * Object2 will use Texture if not set to 0 or another.
            */
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texture;
        }

        public void Translate(Vector3 translation)
        {
            Position += translation;
        }

        public void Rotate(Vector3 rotation)
        {
            Rotation += rotation;
        }

        public void Scale(Vector3 scaling)
        {
            Scaling += scaling;
        }

        public void UpdateModelMatrix()
        {
            ModelMatrix = 
                Matrix4.CreateRotationX(Rotation.X) *
                Matrix4.CreateRotationY(Rotation.Y) *
                Matrix4.CreateRotationZ(Rotation.Z) *
                Matrix4.CreateTranslation(Position) *
                Matrix4.CreateScale(Scaling);
        }
    }
}
