using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using StbImageSharp;
using System.Reflection.Metadata;
using System.IO;
using OpenTK.Mathematics;
using System.Xml.Schema;

namespace Gravsim
{


    public class GEMengine(int width, int height, string title) : GameWindow(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
    {
        static Random rand = new Random();
        float time = 0f;


        Shader shader;

        int VBO;
        int VAO;
        int EBO;

        
        // represents the dimensions of the particles
        public float[] vertices = 
        {
        0.001f,  0.001f, 0.0f,
        0.001f, -0.001f, 0.0f,
       -0.001f, -0.001f, 0.0f,
       -0.001f,  0.001f, 0.0f,
        };

        public uint[] indices =
        {
        0, 1, 3,
        1, 2, 3
        };

        //represents the amount
        List<Matrix4> objects = new List<Matrix4>();

        //represents x and y acceleration values
        List<float> xinertia = new List<float>();
        List<float> yinertia = new List<float>();

        //represents postion of particles
        List<float> xvector = new List<float>();
        List<float> yvector = new List<float>();

        //represents the previous postions of vectors and are used to calculate postions 
        List<float> xvectorOld = new List<float>();
        List<float> yvectorOld = new List<float>();

        //allows one to set specific intial velocities | RandomVelocitySwitch must be off for this to be on
        bool ManualVelocityInputs = true;

        //allows one to set specific intial positions
        bool ManualPositionInputs = true;

        //adds random intial velocity | true for yes and false for no
        bool RandomVelocitySwitch = false;

        //changes random range of intial velocity | interger is multiplied by 0.0001
        int RandomVelocityRange = 10;

        //changes random range of paritcle's starting X vector | the camera view ranges from (-1, 1) for x and y vectors | interger is multiplied by 0.001
        int RandomXRange = 500;

        //changes random range of paritcle's starting Y vector | the camera view ranges from (-1, 1) for x and y vectors | interger is multiplied by 0.001
        int RandomYRange = 500;

        //changes the amount of bodies that the simulation will have | bodies will start counting from 0 so they have to be subracted by 1 | example: (3 - 1) = 2 = true bodyAmount 
        int bodyAmount = 2 - 1;

        //adds the particle's data in the console log | true for yes and false for no
        bool infoSwitch = false;

        //mass of particles
        float mass = 1f;

        //simulation gravity constant
        float GConstant = 0.0005f;

        //adds a stabilizer that makes the gravity simulation more stable at the cost it being not as accurate to true gravity calculations | true for yes and false for no
        bool stabilizerSwitch = false;

        //changes stabilizer constant 
        float stabilizerConstant = 0.000008f;

        // makes it so the force never goes above the | used when the stabilizer is off | the higher the value, the lower the max force can be
        float formulaLimit = 1f;

        // leave false
        bool l = false;
        


        public static void Main()
        {
            using (GEMengine win = new GEMengine(1000, 1000, "GravitySim"))
            {
                win.Run();
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
           
            if(l == true)
            {
                //adds the Force from gravity function to the inertia/acceleration
                for (int i = 0; i <= bodyAmount;)
                {
                    xinertia[i] += GravityX(i);
                    yinertia[i] += GravityY(i);
                    i++;
                }
                
                if(infoSwitch == false)
                {
                    //writes data in the console log
                    for (int i = 0; i <= bodyAmount;)
                    {
                        Console.WriteLine("x vector (" + (i + 1) + "): " + xvector[i]);
                        Console.WriteLine("y vector (" + (i + 1) + "): " + yvector[i]);
                        Console.WriteLine("----------------------------");
                        Console.WriteLine("x inertia (" + (i + 1) + "): " + xinertia[i]);
                        Console.WriteLine("y inertia (" + (i + 1) + "): " + yinertia[i]);
                        Console.WriteLine("----------------------------");
                        Console.WriteLine("time: " + time);
                        Console.WriteLine("----------------------------");
                        i++;
                    }
                }     
            }

            time += 0.008335f;

            //updates the old vectors varibles
            for (int i = 0; i<= bodyAmount;)
            {
                xvectorOld[i] = xvector[i];
                yvectorOld[i] = yvector[i];
                i++;
            }
        }

        protected override void OnLoad()
        {

            Vector4 vec = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            Matrix4 trans = Matrix4.CreateTranslation(1f, 1f, 0.0f);

            // adds all objects and varibles based on inputed bodyAmount varible 
            for(int i = 0; i <= bodyAmount;)
            {
                objects.Add(new Matrix4());

                xinertia.Add(new float());
                yinertia.Add(new float());

                xvector.Add(new float());
                yvector.Add(new float());

                xvectorOld.Add(new float());
                yvectorOld.Add(new float());
                i++;
            }

            //randomizes staring values
            for(int i = 0; i <= bodyAmount;)
            {
                //this is where manual velocites are set | each x and y velocites must be manually set starting from 0 to the true BodyAmount
                if (ManualVelocityInputs == true)
                {
                    xinertia[0] += -0.00043f;
                    yinertia[0] += 0f;

                    xinertia[1] += 0.00043f;
                    yinertia[1] += 0f;


                }
                //this is where manual positions are set | each x and y position must be manually set starting from 0 to the true BodyAmount
               
                if (ManualPositionInputs == true)
                {
                    xvector[0] += 0f;
                    yvector[0] += 0.06f;

                    xvector[1] += 0f;
                    yvector[1] += -0.06f;


                }

                
                if (RandomVelocitySwitch == true)
                {
                    xinertia[i] += (rand.Next(-RandomVelocityRange, RandomVelocityRange)) * 0.0001f;
                    yinertia[i] += (rand.Next(-RandomVelocityRange, RandomVelocityRange)) * 0.0001f;
                }

                if (ManualPositionInputs == false)
                {
                    xvector[i] = (rand.Next(-RandomXRange, RandomXRange)) * 0.001f;
                    yvector[i] = (rand.Next(-RandomYRange, RandomYRange)) * 0.001f; 
                }
                

                xvectorOld[i] = xvector[i];
                yvectorOld[i] = yvector[i];

                i++;
            }
   

            base.OnLoad();

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            EBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            shader = new Shader("shader.vert", "shader.frag");
            shader.Use();
            VSync = VSyncMode.On;
            
            l = true;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            var transform = Matrix4.Identity;
            GL.Clear(ClearBufferMask.ColorBufferBit);

            //changes particle postions based on the acceleration/inertia
            for (int i = 0; i <= bodyAmount;)
            {
                objects[i] = transform * Matrix4.CreateTranslation(xvector[i] += xinertia[i], yvector[i] += yinertia[i], 0.0f);
                i++;
            }

            shader.Use();

            GL.BindVertexArray(VAO);

            //draws particles
            for (int i = 0; i <= bodyAmount;)
            {
                shader.SetMatrix4("transform", objects[i]);
                GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
                i++;
            }
            
            SwapBuffers();
        }

        //calculates the x net forces 
        public float GravityX(int z)
        {
            float Gravity = 0;
            for(int i = 0; i <= bodyAmount;)
            {
                if (i != z)
                {
                    double Gformula;
                    double g = 0;

                    float x1 = xvectorOld[i];
                    float x2 = xvectorOld[z];
                    float y1 = yvectorOld[i];
                    float y2 = yvectorOld[z];

                    var distanceX = x2 - x1;
                    var distanceY = y2 - y1;
                    var distanceMultX = distanceX * distanceX;
                    var distanceMultY = distanceY * distanceY;
                    var distance = distanceMultX + distanceMultY;
                    var distanceSqr = Math.Pow(distanceMultX + distanceMultY, 2);
                    Gformula = mass / distance;
                    var Force = Gformula * GConstant;
                    var p = Math.Sqrt(distance);
                    g = distanceX / p;    
                    var m = Force * g * GConstant;
                    float ForceXConversion = System.Convert.ToSingle(m);

                    if (-ForceXConversion > 0 && stabilizerSwitch == true)
                    {
                        ForceXConversion += -stabilizerConstant;
                    }
                    if (-ForceXConversion < 0 && stabilizerSwitch == true)
                    {
                        ForceXConversion += stabilizerConstant;
                    }

                    Gravity += -ForceXConversion;
                }
                i++;
            }

            return Gravity;
        }

        //calculates the y net forces 
        public float GravityY(int z)
        {
            float Gravity = 0;
            for (int i = 0; i <= bodyAmount;) //objects.Count
            {
                if (i != z)
                {
                    double Gformula;
                    double g = 0;

                    float x1 = xvectorOld[i];
                    float x2 = xvectorOld[z];
                    float y1 = yvectorOld[i];
                    float y2 = yvectorOld[z];

                    var distanceX = x2 - x1;
                    var distanceY = y2 - y1;
                    var distanceMultX = distanceX * distanceX;
                    var distanceMultY = distanceY * distanceY;
                    var distance = distanceMultX + distanceMultY;
                    var distanceSqr = Math.Pow(distance, 2);
                    Gformula = mass / distance;
                    var Force = Gformula * GConstant;
                    var p = Math.Sqrt(distance);
                    g = distanceY / p;

                    var m = Force * g * GConstant;
                    float ForceYConversion = System.Convert.ToSingle(m);

                    if(-ForceYConversion > 0 && stabilizerSwitch == true)
                    {
                    ForceYConversion += -stabilizerConstant;
                    }
                    if (-ForceYConversion < 0 && stabilizerSwitch == true)
                    {
                    ForceYConversion += stabilizerConstant;
                    }

                    Gravity += -ForceYConversion;
                }
                i++;
            }

            return Gravity;
        }


        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            shader.Dispose();
        }
    }

    public class Shader
    {
        int Handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            string VertexShaderSource = File.ReadAllText(vertexPath);

            string FragmentShaderSource = File.ReadAllText(fragmentPath);

            var VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);
            GL.CompileShader(VertexShader);
            
            var FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);
            GL.CompileShader(FragmentShader);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);

            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }

        public void Use()
        {

            GL.UseProgram(Handle);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            int location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, true, ref data);
        }
    }



}



    

    
