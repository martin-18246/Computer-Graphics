using System;
using System.Diagnostics;
using CommandLine;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Globalization;
using System.Text;
using Util;
using System.Transactions;
using System.CodeDom.Compiler;
using System.Security;
using Silk.NET.Vulkan;
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace _08_Fireworks;

using Vector3 = Vector3D<float>;
using Matrix4 = Matrix4X4<float>;

public static class DebuggerClass
{
    // list of launchers that the user can use during the simulation
    public static List<Launcher> globalLaunchers = new List<Launcher>();
    public static int currentIndex = 0;

    // changing which launcher is selected by the user using arrows
    public static void ChangeIndex(bool isLeft)
    {
        if (isLeft)
        {
            currentIndex = (currentIndex + (globalLaunchers.Count - 1)) % globalLaunchers.Count;
        }

        else
        {
            currentIndex = (currentIndex + 1) % globalLaunchers.Count;
        }
    }
}

public class Options
{
  [Option('w', "width", Required = false, Default = 1500, HelpText = "Window width in pixels.")]
  public int WindowWidth { get; set; } = 1500;

  [Option('h', "height", Required = false, Default = 900, HelpText = "Window height in pixels.")]
  public int WindowHeight { get; set; } = 900;

  [Option('p', "particles", Required = false, Default = 10000, HelpText = "Maximum number of particles.")]
  public int Particles { get; set; } = 10000;

  [Option('r', "rate", Required = false, Default = 1000.0, HelpText = "Particle generation rate per second.")]
  public double ParticleRate { get; set; } = 1000.0;

  [Option('t', "texture", Required = false, Default = ":check:", HelpText = "User-defined texture.")]
  public string TextureFile { get; set; } = ":check:";
}

public class Explosion
{
    // how many new particles will be generated during the explosion
    public int numberOfShards { get; set; }

    // initial velocity passed to the new particles
    public float explosionStrength { get; set; }
    public float initialExplosionStrength { get; set; }

    public Random randExp = new Random();
    public Simulation simulation { get; set; }

    // introducing randomness into the explosion strength: 0.2 means particles will be shot at 80% to 120% of explosion strength
    public float explosionStrengthRandomPercentModifier { get; set; }

    private List<ParticleType>? listParticleTypes;
    private int numParticleTypes;
    private float strengthDifference;
    private float minimumExplosionStrength;
    private float maximumExplosionStrength;

    

    public Explosion (int numberOfShards, float explosionStrength, float explosionStrengthRandomPercentModifier, List<ParticleType>? particleTypes, Simulation simulation)
    {
        this.numberOfShards = numberOfShards;
        this.explosionStrength = explosionStrength;
        this.initialExplosionStrength = this.explosionStrength;
        this.simulation = simulation;
        this.explosionStrengthRandomPercentModifier = explosionStrengthRandomPercentModifier;
        this.listParticleTypes = particleTypes;
        numParticleTypes = (listParticleTypes != null) ? listParticleTypes.Count : 0;
        strengthDifference = explosionStrength * explosionStrengthRandomPercentModifier;
        minimumExplosionStrength = explosionStrength - strengthDifference;
        maximumExplosionStrength = explosionStrength + strengthDifference;
    }

    public void Explode(Vector3 explosionPosition)
    {
        if (listParticleTypes == null || numberOfShards == 0 || numParticleTypes == 0) return;

        // generate all shards
        for (int i = 0; i < numberOfShards; i++)
        {
            float x = (float)randExp.NextDouble() * 2 - 1;
            float y = (float)randExp.NextDouble() * 2 - 1;
            float z = (float)randExp.NextDouble() * 2 - 1;
            Vector3 explosionDirection = new Vector3(x, y, z);
            float explosionVectorLength = (float)Math.Sqrt(x * x + y * y + z * z);
            Vector3 explosionDirectionNormalized;
            if (explosionVectorLength != 0)
            {
                explosionDirectionNormalized = new Vector3(explosionDirection.X / explosionVectorLength, explosionDirection.Y / explosionVectorLength, explosionDirection.Z / explosionVectorLength);
            }

            else
            {
                return;
            }

            if (explosionStrengthRandomPercentModifier != 0f)
            {
                explosionStrength = minimumExplosionStrength + (float)randExp.NextDouble() * (maximumExplosionStrength - minimumExplosionStrength);
            }

            Vector3 initialVelocity = explosionDirectionNormalized * (float)explosionStrength;
            

            simulation.Generate(initialVelocity, listParticleTypes[i % numParticleTypes], explosionPosition);
            explosionStrength = initialExplosionStrength;
        }

    }
}

public class Launcher
{
    public Vector3 Position { get; set; }
    // how often will the launcher shoot out particles
    public double? launchInterval {  get; set; }
    public double lastLaunchTime { get; set;  } = 0f;
    public bool HasFired = false;

    // initial velocity passed to the launched particles
    public Vector3 fireVelocity { get; set; }
    public Simulation sim;
    private Random rand = new Random();
    // which particles will the launcher shoot out
    private ParticleType particleType;

    public double timeNow;
    public int? numShotsCircular;
    public int? angleCircularShots;
    public float randomnessDirectionSquareSize;
    public int particlesPerLaunch;

    public Launcher(Vector3 position, double? launchInterval, Simulation sim, ParticleType particleType, Vector3 fireVelocity, float randomnessDirection, int? numShotsCircular, int? angleCircularShots, int particlesPerLaunch)
    {
        Position = position;

        this.launchInterval = launchInterval;

        this.sim = sim;
        this.particleType = particleType;
        this.fireVelocity = fireVelocity;
        this.numShotsCircular = numShotsCircular;
        this.angleCircularShots = angleCircularShots;
        randomnessDirectionSquareSize = randomnessDirection;
        this.particlesPerLaunch = particlesPerLaunch;
    }

    public void HandleFrame(double timeNow)
    {
        this.timeNow = timeNow;

        if (!HasFired && launchInterval == null)
        {
            Fire();
        }

        if (launchInterval == null)
        {
            return;
        }

        if (timeNow - lastLaunchTime > launchInterval)
        {
            lastLaunchTime = timeNow;
            if (numShotsCircular == null)
            {
                for (int i = 0; i < particlesPerLaunch; i++)
                {
                    Fire();
                }
            }

            else
            {
                CircularShot();
            }
            
        }

    }

    public void Fire()
    {
        float minimumDifference = 0 - (randomnessDirectionSquareSize / 2);
        float maximumDifference = minimumDifference * -1;
        float offsetX = minimumDifference + (float)rand.NextDouble() * (maximumDifference - minimumDifference);
        float offsetZ = minimumDifference + (float)rand.NextDouble() * (maximumDifference - minimumDifference);
        HasFired = true;
        Vector3 fireAtVelocity = fireVelocity;

        fireAtVelocity.X += offsetX;
        fireAtVelocity.Z += offsetZ;
        sim.Generate(fireAtVelocity, particleType, Position);

    }

    public void CircularShot()
    {
        Vector3 fireAtVelocity = fireVelocity;
        if (numShotsCircular == null || angleCircularShots == null) return;
        // count the difference in velocity in the other directions using trigonometry
        // calculate the angle difference between two shots
        float deltaAngle = 360f / (float)numShotsCircular;
        float currentAngle = 0f;
        
        for (int shot = 0; shot < numShotsCircular; shot++)
        {
            currentAngle = shot * deltaAngle;
            float deltaX = (float)Math.Cos(Program.degToRad(currentAngle));
            float deltaZ = (float)Math.Sin(Program.degToRad(currentAngle));
            fireAtVelocity.X += deltaX * (float)angleCircularShots;
            fireAtVelocity.Z += deltaZ * (float)angleCircularShots;
            sim.Generate(fireAtVelocity, particleType, Position);
        }
    }
}

public class CometTail
{
    public bool hasTail;
    public int framesPerParticle;
    public ParticleType? particleType;
    public Simulation sim;

    public CometTail(bool hasTail, int framesPerParticle, ParticleType? particleType, Simulation sim)
    {
        this.hasTail = hasTail;
        this.framesPerParticle = framesPerParticle;
        this.particleType = particleType;
        this.sim = sim;
    }

    public void DrawTail(Vector3 particlePosition)
    {
        if (particleType != null)
        {
            sim.Generate(new Vector3(0f, 0f, 0f), particleType.Value, particlePosition);
        }
    }
}

public struct ParticleType
{
    public Vector3 Color { get; set; }
    public float Size { get; set; }
    public float Mass { get; set; } = 0.1f;
    public double MaximumAge { get; set; }

    public float LinearDragCoefficient { get; set; } = 0.01f;
    public float QuadraticDragCoefficient { get; set; } = 0.01f;

    // how will the particle explode when it dies
    public Explosion explosion { get; set; }

    // the faster the particle, the whiter it is
    public bool enableColorBasedOnSpeed;

    public Vector3? Gravity = new Vector3(0f, -9.81f, 0f);

    // which trajectory will the particle leave behind
    public CometTail? tail { get; set; }

    public ParticleType(Vector3 color, float size, float mass, double maximumAge, Explosion explosion, CometTail? tail, float linearDrag, float quadraticDrag, bool speedColor, Vector3? gravity)
    {
        Color = color;
        Size = size;
        Mass = mass;
        MaximumAge = maximumAge;
        this.explosion = explosion;
        this.tail = tail;
        this.LinearDragCoefficient = linearDrag;
        this.QuadraticDragCoefficient = quadraticDrag;
        this.enableColorBasedOnSpeed = speedColor;
        Gravity = gravity;
    } 
}

public class Particle
{
    public Simulation sim;
    private Vector3 Gravity = new Vector3(0.0f, -9.81f, 0.0f);
    //private Vector3 InitialGravity = new Vector3(0.0f, -9.81f, 0.0f);
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 Color { get; set; }
    public float Size { get; set; }
    public float Mass { get; set; } = 0.1f;
    public double Age { get; set; } = 0f;
    public double MaximumAge { get; set; } // 
    public float LinearDragCoefficient { get; set; } = 0.01f;
    public float QuadraticDragCoefficient { get; set; } = 0.01f;
    public Explosion explosion { get; set; }

    public double timeUntilExplosion;

    private double SimulatedTime;

    // if the particle will change color depending on speed
    public bool enableColorBasedOnSpeed;

    public float initialSpeedScalar;
    public CometTail? tail { get; set; }
    public int framesSinceLastTailParticle = 0;

    public Vector3 colorDifference;


    /// Create a new particle.
    public Particle(double now, Vector3 launchVelocity, Simulation simulation, ParticleType particleType, Vector3 initialPosition)
    {
        //Reset(now);
        Position = initialPosition;
        Color = particleType.Color;
        Size = particleType.Size;
        MaximumAge = particleType.MaximumAge;
        SimulatedTime = now;
        sim = simulation;
        Velocity += launchVelocity;
        explosion = particleType.explosion;
        if (particleType.tail != null)
        {
            this.tail = particleType.tail;
        }
        this.LinearDragCoefficient = particleType.LinearDragCoefficient;
        this.QuadraticDragCoefficient = particleType.QuadraticDragCoefficient;
        this.enableColorBasedOnSpeed = particleType.enableColorBasedOnSpeed;

        initialSpeedScalar = scalarSpeed(Velocity);
        colorDifference = new Vector3(1f - Color.X, 1f - Color.Y, 1f - Color.Z);
        if (particleType.Gravity == null)
        {

        }

        else
        {
            Gravity = particleType.Gravity.Value;
        }

        
    }

    public void Explode()
    {

        explosion.Explode(Position);
        
    }


    /// Simulate one step in time.
    public bool SimulateTo(double time)
    {
        if (time <= SimulatedTime) return true;


        double dt = time - SimulatedTime; // time difference between the last step
        SimulatedTime = time;
        Vector3 acceleration = Gravity;
        Vector3 dragForce = -LinearDragCoefficient * Velocity - QuadraticDragCoefficient * Velocity * Velocity;
        //Vector3 dragForce = -LinearDragCoefficient * Velocity;
        acceleration += dragForce / Mass;

        // Update velocity and position
        Velocity += acceleration * (float)dt;

        Position += Velocity * (float)dt;

        Age += dt;
        if (Age > MaximumAge)
        {
            Explode();
            return false;
        }

        // Change particle color.
        if (Age < 6.0)
            Color *= (float)Math.Pow(0.8, dt);

        // Change particle size.
        if (Age < 5.0)
            Size *= (float)Math.Pow(0.8, dt);

        // Draw tail
        if (tail != null && framesSinceLastTailParticle == 0)
        {
            tail.DrawTail(Position);
        }
        framesSinceLastTailParticle++;

        if (tail != null) framesSinceLastTailParticle %= tail.framesPerParticle;


        // change tail color
        //if (enableColorBasedOnSpeed)
        //{

        //}

        // change color

        if (enableColorBasedOnSpeed)
        {
            float percentage = scalarSpeed(Velocity) / initialSpeedScalar;
            percentage = 1 - percentage;
            Color = new Vector3(1f - colorDifference.X * percentage, 1f - colorDifference.Y * percentage, 1f - colorDifference.Z * percentage);
        }

        return true;
    }

    public float scalarSpeed(Vector3 velocity)
    {
        return (float)Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2) + Math.Pow(velocity.Z, 2));
    }

    public void FillBuffer (float[] buffer, ref int i)
    {
        // offset  0: Position
        buffer[i++] = Position.X;
        buffer[i++] = Position.Y;
        buffer[i++] = Position.Z;

        // offset  3: Color
        buffer[i++] = Color.X;
        buffer[i++] = Color.Y;
        buffer[i++] = Color.Z;

        // offset  6: Normal
        buffer[i++] = 0.0f;
        buffer[i++] = 1.0f;
        buffer[i++] = 0.0f;

        // offset  9: Txt coordinates
        buffer[i++] = 0.5f;
        buffer[i++] = 0.5f;

        // offset 11: Point size
        buffer[i++] = Size;
    }
}

public class Simulation
{
    private List<Particle> particles = new();

    public int Particles => particles.Count;

    public List<Launcher> launchers = new();

    public int MaxParticles { get; private set; }

    private double SimulatedTime;
    public double ParticleRate { get; set; }


    public Simulation (double now, double particleRate, int maxParticles, int initParticles)
    {
        SimulatedTime = now;
        ParticleRate = particleRate;
        MaxParticles = maxParticles;
        
        // default necessary instances
        Explosion emptyExplosion = new Explosion(0, 5f, 0, null, this);
        CometTail NoTail = new CometTail(false, 5, null, this);

        // all other instances
        ParticleType blueTailParticle = new ParticleType(new Vector3(0.2f, 0.5f, 0.5f), 2f, 0.5f, 10f, emptyExplosion, null, 10f, 10f, false, null);
        ParticleType blueTailParticle2 = new ParticleType(new Vector3(0.2f, 0.5f, 0.5f), 2f, 0.5f, 0.5f, emptyExplosion, null, 10f, 10f, false, new Vector3(0f, 0f, 0f));
        ParticleType orangeFireSparkle = new ParticleType(new Vector3(1f, 0.5f, 0.3f), 3f, 0.5f, 0.2f, emptyExplosion, NoTail, 0f, 0f, false, null);
        ParticleType orangeFireSparkle2 = new ParticleType(new Vector3(1f, 0.9f, 0.3f), 1f, 0.5f, 0.2f, emptyExplosion, NoTail, 0f, 0f, false, null);



        float smallLinearDrag = 1.1f;
        float smallQuadraticDrag = 0f;
        float smallMass = 1f;
        Vector3 smallParticleGravity = new Vector3(0f, -0.81f, 0f);
        float smallMaximumAge = 0.9f;

        ParticleType smallOrangeNonexplosive = new ParticleType(new Vector3(5.92f, 0.42f, 0.4f), 1f, smallMass, smallMaximumAge, emptyExplosion, NoTail, smallLinearDrag, smallQuadraticDrag, false, smallParticleGravity);
        ParticleType smallGreenNonexplosive = new ParticleType(new Vector3(5.26f, 0.91f, 0.49f), 1f, smallMass, smallMaximumAge, emptyExplosion, NoTail, smallLinearDrag, smallQuadraticDrag, false, smallParticleGravity);
        ParticleType smallGreen2Nonexplosive = new ParticleType(new Vector3(0.416f, 0.871f, 0.412f), 1f, smallMass, smallMaximumAge, emptyExplosion, null, smallLinearDrag, smallQuadraticDrag, false, smallParticleGravity);
        ParticleType smallPinkNonexplosive = new ParticleType(new Vector3(0.859f, 0.388f, 0.627f), 1f, smallMass, smallMaximumAge, emptyExplosion, null, smallLinearDrag, smallQuadraticDrag, false, smallParticleGravity);
        ParticleType smallBlueNonexplosive = new ParticleType(new Vector3(0.859f, 0.388f, 1f), 1f, smallMass, smallMaximumAge, emptyExplosion, null, smallLinearDrag, smallQuadraticDrag, false, smallParticleGravity);
        ParticleType smallRedNonexplosive = new ParticleType(new Vector3(1f, 0.388f, 0.5f), 1f, smallMass, smallMaximumAge, emptyExplosion, null, smallLinearDrag, smallQuadraticDrag, false, smallParticleGravity);
        ParticleType mediumBlueNonexplosive = new ParticleType(new Vector3(0.2f, 0.2f, 1f), 2f, smallMass, smallMaximumAge, emptyExplosion, null, smallLinearDrag, smallQuadraticDrag, false, smallParticleGravity);
        ParticleType mediumRedNonexplosive = new ParticleType(new Vector3(1.2f, 0.2f, 0.2f), 2f, smallMass, smallMaximumAge, emptyExplosion, null, smallLinearDrag, smallQuadraticDrag, false, smallParticleGravity);
        ParticleType mediumBlueNonexplosive2 = new ParticleType(new Vector3(0.0f, 0.0f, 1f), 4f, smallMass, smallMaximumAge, emptyExplosion, null, smallLinearDrag, smallQuadraticDrag, false, smallParticleGravity);
        ParticleType mediumRedNonexplosive2 = new ParticleType(new Vector3(0.5f, 0.0f, 0f), 4f, smallMass, smallMaximumAge, emptyExplosion, null, smallLinearDrag, smallQuadraticDrag, false, smallParticleGravity);
        
        

        
        Explosion complementaryOrangeGreen = new Explosion(850, 4f, 0.2f, new List<ParticleType> { smallOrangeNonexplosive, smallGreenNonexplosive }, this);
        Explosion complementaryBluePink = new Explosion(850, 3f, 0.5f, new List<ParticleType> { smallGreen2Nonexplosive, smallPinkNonexplosive }, this);
        Explosion smallBlue = new Explosion(150, 1.5f, 0f, new List<ParticleType> { smallBlueNonexplosive }, this);



        CometTail SimpleTail = new CometTail(true, 1, blueTailParticle, this);
        CometTail SimpleTailStationary = new CometTail(true, 1, blueTailParticle2, this);


        ParticleType basicExplosiveShot = new ParticleType(new Vector3(0.7f, 0.2f, 0.2f), 10f, 1f, 0.5f, complementaryOrangeGreen, SimpleTailStationary, 0.1f, 0.03f, true, null);
        ParticleType basicShortExplosiveShot = new ParticleType(new Vector3(0.5f, 0.2f, 0.2f), 3f, 2f, 0.8f, smallBlue, NoTail, 0.1f, 0.03f, true, new Vector3(0f, -4f, 0f));
        ParticleType LongExplosiveBlue = new ParticleType(new Vector3(0.2f, 0.2f, 0.8f), 3f, 2f, 0.3f, complementaryBluePink, NoTail, 0.1f, 0.03f, true, new Vector3(0f, 0f, 0f));
        Explosion explosiveParticleExplosion = new Explosion(5, 2.5f, 0.5f, new List<ParticleType> { LongExplosiveBlue }, this);

        Explosion hugeBlueExplosion = new Explosion(1650, 12f, 0.2f, new List<ParticleType> { mediumBlueNonexplosive2, smallBlueNonexplosive, mediumBlueNonexplosive }, this);
        Explosion hugeRedExplosion = new Explosion(1650, 12f, 0.2f, new List<ParticleType> { mediumRedNonexplosive2, smallRedNonexplosive, mediumRedNonexplosive }, this);

        ParticleType hugeBlueExplosive = new ParticleType(new Vector3(0.2f, 0.2f, 0.8f), 12f, 2f, 0.5f, hugeBlueExplosion, NoTail, 0.1f, 0.03f, true, new Vector3(0f, 0f, 0f));
        ParticleType hugeRedExplosive = new ParticleType(new Vector3(0.8f, 0.2f, 0.2f), 12f, 2f, 0.5f, hugeRedExplosion, NoTail, 0.1f, 0.03f, true, new Vector3(0f, 0f, 0f));

        ParticleType DoubleExplosive = new ParticleType(new Vector3(0.1f, 0.9f, 0.2f), 7f, 0.2f, 0.5f, explosiveParticleExplosion, NoTail, 0.1f, 0.03f, true, null);
        
        Launcher fireSparklesLauncher = new Launcher(new Vector3(0f, -1f, 0f), 0.01f, this, orangeFireSparkle, new Vector3(0f, 2f, 0f), 1.2f, null, null, 4);
        Launcher fireSparklesLauncher2 = new Launcher(new Vector3(0f, -1f, 0f), 0.01f, this, orangeFireSparkle2, new Vector3(0f, 2.3f, 0f), 1f, null, null, 4);
        Launcher basicExplosiveShotLauncher = new Launcher(new Vector3(0f, -1f, 0f), 3f, this, basicExplosiveShot, new Vector3(0f, 10f, 0f), 5f, null, null, 3);
        Launcher basicSmallExplosiveLauncher = new Launcher(new Vector3(0f, -1f, 0f), 1f, this, basicShortExplosiveShot, new Vector3(0f, 5f, 0f), 2f, null, null, 1);
        Launcher bigDoubleExplosiveLauncher = new Launcher(new Vector3(0f, -1f, 0f), 4f, this, DoubleExplosive, new Vector3(0f, 15f, 0f), 0f, null, null, 1);
        Launcher hugeBlueExplosiveShotLauncher = new Launcher(new Vector3(0f, -1f, 0f), 5f, this, hugeBlueExplosive, new Vector3(0f, 10f, 0f), 0f, null, null, 1);
        Launcher hugeRedExplosiveShotLauncher = new Launcher(new Vector3(0f, -1f, 0f), 6f, this, hugeRedExplosive, new Vector3(0f, 10f, 0f), 0f, null, null, 1);


        launchers.Add(fireSparklesLauncher); // fire emulation
        launchers.Add(fireSparklesLauncher2); // fire emulation
        launchers.Add(basicSmallExplosiveLauncher); // fires small pink explosive particles
        launchers.Add(basicExplosiveShotLauncher); // fires a classic shot that will explode in red and orange particles, the trajectory is turned on
        launchers.Add(bigDoubleExplosiveLauncher); // fires a shot that will explode in smaller shots that will also explode
        launchers.Add(hugeBlueExplosiveShotLauncher); // fires a big shot that will create a big blue explosion
        launchers.Add(hugeRedExplosiveShotLauncher); // fires a big shot that will create a red blue explosion

        DebuggerClass.globalLaunchers.Add(basicSmallExplosiveLauncher);
        DebuggerClass.globalLaunchers.Add(basicExplosiveShotLauncher);
        DebuggerClass.globalLaunchers.Add(bigDoubleExplosiveLauncher);
        DebuggerClass.globalLaunchers.Add(hugeBlueExplosiveShotLauncher);
        DebuggerClass.globalLaunchers.Add(hugeRedExplosiveShotLauncher);



    }

    public void Generate(Vector3 launchVelocity, ParticleType particleType, Vector3 initialPosition)
    {
        particles.Add(new Particle(SimulatedTime, launchVelocity, this, particleType, initialPosition));

    }

    public void SimulateTo(double time)
    {
        if (time <= SimulatedTime) return;

        double dt = time - SimulatedTime;
        SimulatedTime = time;

        // 1. Simulate all the particles.
        List<int> toRemove = new();
        for (int i = 0; i < particles.Count; i++)
        {
            // Simulate one particle.
            if (!particles[i].SimulateTo(time))
            {
                // Delete the particle.
                toRemove.Add(i);
            }
        }

        // 2. Remove the dead ones.
        for (var i = toRemove.Count; --i >= 0; ) particles.RemoveAt(toRemove[i]);


        // 3. the launchers handle launching
        for (int i = 0; i < launchers.Count; i++)
        {
            launchers[i].HandleFrame(time);
        }


    }


  public int FillBuffer (float[] buffer)
  {
    int i = 0;
    foreach (var p in particles)
      p.FillBuffer(buffer, ref i);

    return particles.Count;
  }

  /// Removes all the particles.
  public void Reset()
  {
    particles.Clear();
  }
}

internal class Program
{
  // System objects.
  private static IWindow? window;
  private static GL? Gl;

  // VB locking (too lousy?)
  private static object renderLock = new();

  // Window size.
  private static float width;
  private static float height;

  // Trackball.
  private static Trackball? tb;

  // FPS counter.
  private static FPS fps = new();

  // Scene dimensions.
  private static Vector3 sceneCenter = Vector3.Zero;
  private static float sceneDiameter = 1.5f;

  // Global 3D data buffer.
  private const int MAX_VERTICES = 65536;
  private const int VERTEX_SIZE = 12;     // x, y, z, R, G, B, Nx, Ny, Nz, s, t, size

  /// <summary>
  /// Current dynamic vertex buffer in .NET memory.
  /// Better idea is to store the buffer on GPU and update it every frame.
  /// </summary>
  private static float[] vertexBuffer = new float[MAX_VERTICES * VERTEX_SIZE];

  /// <summary>
  /// Current number of vertices to draw.
  /// </summary>
  private static int vertices = 0;

  public static int maxParticles = 0;
  public static double particleRate = 1000.0;

  private static BufferObject<float>? Vbo;
  private static VertexArrayObject<float>? Vao;

  // Texture.
  private static Util.Texture? texture;
  private static bool useTexture = false;
  private static string textureFile = ":check:";
  private const int TEX_SIZE = 128;

  // Lighting.
  private static bool usePhong = false;

  // Shader program.
  private static ShaderProgram? ShaderPrg;

  private static double nowSeconds = FPS.NowInSeconds;

  // Particle simulation system.
  private static Simulation? sim;

  //////////////////////////////////////////////////////
  // Application.

    public static float degToRad(float degs)
    {
        return (degs * (float)((2 * Math.PI) / 360f));
    }

    public static float radToDeg(float rads)
    {
        return (rads * (float)((360f) / (2 * Math.PI)));
    }

  private static string WindowTitle()
  {
    StringBuilder sb = new("08-Fireworks");

    if (sim != null)
    {
      sb.Append(string.Format(CultureInfo.InvariantCulture, " [{0} of {1}], rate={2:f0}", sim.Particles, sim.MaxParticles, sim.ParticleRate));
    }

    sb.Append(string.Format(CultureInfo.InvariantCulture, ", fps={0:f1}", fps.Fps));
    if (window != null &&
        window.VSync)
      sb.Append(" [VSync]");

    double pps = fps.Pps;
    if (pps > 0.0)
      if (pps < 5.0e5)
        sb.Append(string.Format(CultureInfo.InvariantCulture, ", pps={0:f1}k", pps * 1.0e-3));
      else
        sb.Append(string.Format(CultureInfo.InvariantCulture, ", pps={0:f1}m", pps * 1.0e-6));

    if (tb != null)
    {
      sb.Append(tb.UsePerspective ? ", perspective" : ", orthographic");
      sb.Append(string.Format(CultureInfo.InvariantCulture, ", zoom={0:f2}", tb.Zoom));
    }

    if (useTexture &&
        texture != null &&
        texture.IsValid())
      sb.Append($", txt={texture.name}");
    else
      sb.Append(", no texture");

    if (usePhong)
      sb.Append(", Phong shading");

    return sb.ToString();
  }

  private static void SetWindowTitle()
  {
    if (window != null)
      window.Title = WindowTitle();
  }

  private static void Main(string[] args)
  {
    Parser.Default.ParseArguments<Options>(args)
      .WithParsed<Options>(o =>
      {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(o.WindowWidth, o.WindowHeight);
        options.Title = WindowTitle();
        options.PreferredDepthBufferBits = 24;
        options.VSync = true;

        window = Window.Create(options);
        width  = o.WindowWidth;
        height = o.WindowHeight;

        window.Load    += OnLoad;
        window.Render  += OnRender;
        window.Closing += OnClose;
        window.Resize  += OnResize;

        textureFile = o.TextureFile;
        maxParticles = Math.Min(MAX_VERTICES, o.Particles);
        particleRate = o.ParticleRate;

        window.Run();
      });
  }

  private static void VaoPointers()
  {
    Debug.Assert(Vao != null);
    Vao.Bind();
    Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, VERTEX_SIZE,  0);
    Vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, VERTEX_SIZE,  3);
    Vao.VertexAttributePointer(2, 3, VertexAttribPointerType.Float, VERTEX_SIZE,  6);
    Vao.VertexAttributePointer(3, 2, VertexAttribPointerType.Float, VERTEX_SIZE,  9);
    Vao.VertexAttributePointer(4, 1, VertexAttribPointerType.Float, VERTEX_SIZE, 11);
  }

  private static void OnLoad()
  {
    Debug.Assert(window != null);

    // Initialize all the inputs (keyboard + mouse).
    IInputContext input = window.CreateInput();
    for (int i = 0; i < input.Keyboards.Count; i++)
    {
      input.Keyboards[i].KeyDown += KeyDown;
      input.Keyboards[i].KeyUp   += KeyUp;
    }
    for (int i = 0; i < input.Mice.Count; i++)
    {
      input.Mice[i].MouseDown   += MouseDown;
      input.Mice[i].MouseUp     += MouseUp;
      input.Mice[i].MouseMove   += MouseMove;
      input.Mice[i].DoubleClick += MouseDoubleClick;
      input.Mice[i].Scroll      += MouseScroll;
    }

    // OpenGL global reference (shortcut).
    Gl = GL.GetApi(window);

    //------------------------------------------------------
    // Render data.

    // Init the rendering data.
    lock (renderLock)
    {

      sim = new Simulation(nowSeconds, particleRate, maxParticles, maxParticles / 10);
      vertices = sim.FillBuffer(vertexBuffer);

      // Vertex Array Object = Vertex buffer + Index buffer.
      Vbo = new BufferObject<float>(Gl, vertexBuffer, BufferTargetARB.ArrayBuffer);
      Vao = new VertexArrayObject<float>(Gl, Vbo);
      VaoPointers();

      // Initialize the shaders.
      ShaderPrg = new ShaderProgram(Gl, "vertex.glsl", "fragment.glsl");

      // Initialize the texture.
      if (textureFile.StartsWith(":"))
      {
        // Generated texture.
        texture = new(TEX_SIZE, TEX_SIZE, textureFile);
        texture.GenerateTexture(Gl);
      }
      else
      {
        texture = new(textureFile, textureFile);
        texture.OpenglTextureFromFile(Gl);
      }

      // Trackball.
      tb = new(sceneCenter, sceneDiameter);
    }

    // Main window.
    SetWindowTitle();
    SetupViewport();
  }

  private static float mouseCx =  0.001f;

  private static float mouseCy = -0.001f;

  private static void SetupViewport()
  {
    // OpenGL viewport.
    Gl?.Viewport(0, 0, (uint)width, (uint)height);

    tb?.ViewportChange((int)width, (int)height, 0.05f, 20.0f);

    // The tight coordinate is used for mouse scaling.
    float minSize = Math.Min(width, height);
    mouseCx = sceneDiameter / minSize;
    // Vertical mouse scaling is just negative...
    mouseCy = -mouseCx;
  }


  private static void OnResize(Vector2D<int> newSize)
  {
    width  = newSize[0];
    height = newSize[1];
    SetupViewport();
  }


  private static unsafe void OnRender(double obj)
  {
    Debug.Assert(Gl != null);
    Debug.Assert(ShaderPrg != null);
    Debug.Assert(tb != null);

    Gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit);

    lock (renderLock)
    {
      // Simulation the particle system.
      nowSeconds = FPS.NowInSeconds;
      if (sim != null)
      {
        sim.SimulateTo(nowSeconds);
        vertices = sim.FillBuffer(vertexBuffer);
      }

      // Rendering properties (set in every frame for clarity).
      Gl.Enable(GLEnum.DepthTest);
      Gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
      Gl.Disable(GLEnum.CullFace);
      Gl.Enable(GLEnum.VertexProgramPointSize);

      // Draw the scene (set of Object-s).
      VaoPointers();
      ShaderPrg.Use();

      // Shared shader uniforms - matrices.
      ShaderPrg.TrySetUniform("view", tb.View);
      ShaderPrg.TrySetUniform("projection", tb.Projection);
      ShaderPrg.TrySetUniform("model", Matrix4.Identity);

      // Shared shader uniforms - Phong shading.
      ShaderPrg.TrySetUniform("lightColor", 1.0f, 1.0f, 1.0f);
      ShaderPrg.TrySetUniform("lightPosition", -8.0f, 8.0f, 8.0f);
      ShaderPrg.TrySetUniform("eyePosition", tb.Eye);
      ShaderPrg.TrySetUniform("Ka", 0.1f);
      ShaderPrg.TrySetUniform("Kd", 0.7f);
      ShaderPrg.TrySetUniform("Ks", 0.3f);
      ShaderPrg.TrySetUniform("shininess", 60.0f);
      ShaderPrg.TrySetUniform("usePhong", usePhong);

      // Shared shader uniforms - Texture.
      if (texture == null || !texture.IsValid())
        useTexture = false;
      ShaderPrg.TrySetUniform("useTexture", useTexture);
      ShaderPrg.TrySetUniform("tex", 0);
      if (useTexture)
        texture?.Bind(Gl);

      // Draw the particle system.
      vertices = (sim != null) ? sim.FillBuffer(vertexBuffer) : 0;

      if (Vbo != null &&
          vertices > 0)
      {
        Vbo.UpdateData(vertexBuffer, 0, vertices * VERTEX_SIZE);

        // Draw the batch of points.
        Gl.DrawArrays((GLEnum)PrimitiveType.Points, 0, (uint)vertices);

        // Update Pps.
        fps.AddPrimitives(vertices);
      }
    }

    // Cleanup.
    Gl.UseProgram(0);
    if (useTexture)
      Gl.BindTexture(TextureTarget.Texture2D, 0);

    // FPS.
    if (fps.AddFrames())
      SetWindowTitle();
  }


  private static void OnClose()
  {
    Vao?.Dispose();
    ShaderPrg?.Dispose();

    // Remember to dispose the textures.
    texture?.Dispose();
  }


  private static int shiftDown = 0;


  private static int ctrlDown = 0;

  private static void KeyDown(IKeyboard arg1, Key arg2, int arg3)
  {
    if (tb != null &&
        tb.KeyDown(arg1, arg2, arg3))
    {
      SetWindowTitle();
      //return;
    }

    switch (arg2)
    {
      case Key.ShiftLeft:
      case Key.ShiftRight:
        shiftDown++;
        break;

      case Key.ControlLeft:
      case Key.ControlRight:
        ctrlDown++;
        break;

      case Key.T:
        // Toggle texture.
        useTexture = !useTexture;
        if (useTexture)
          Ut.Message($"Texture: {texture?.name}");
        else
          Ut.Message("Texturing off");
        SetWindowTitle();
        break;

      case Key.I:
        // Toggle Phong shading.
        usePhong = !usePhong;
         Ut.Message("Phong shading: " + (usePhong ? "on" : "off"));
        SetWindowTitle();
        break;

      case Key.P:
        // Perspective <-> orthographic.
        if (tb != null)
        {
          tb.UsePerspective = !tb.UsePerspective;
          SetWindowTitle();
        }
        break;

      case Key.C:
        // Reset view.
        if (tb != null)
        {
          tb.Reset();
          Ut.Message("Camera reset");
        }
        break;

      case Key.V:
        // Toggle VSync.
        if (window != null)
        {
          window.VSync = !window.VSync;
          if (window.VSync)
          {
            Ut.Message("VSync on");
            fps.Reset();
          }
          else
            Ut.Message("VSync off");
        }
        break;

      case Key.R:
        // Reset the simulator.
        if (sim != null)
        {
          sim.Reset();
          Ut.Message("Simulator reset");
        }
        break;

      case Key.Up:
        // Increase particle generation rate.
        if (sim != null)
        {
          sim.ParticleRate *= 1.1;
          SetWindowTitle();
        }
        break;

      case Key.Down:
        // Decrease particle generation rate.
        if (sim != null)
        {
          sim.ParticleRate /= 1.1;
          SetWindowTitle();
        }
        break;

      case Key.F1:
        // Help.
        Ut.Message("T           toggle texture", true);
        Ut.Message("I           toggle Phong shading", true);
        Ut.Message("P           toggle perspective", true);
        Ut.Message("V           toggle VSync", true);
        Ut.Message("C           camera reset", true);
        Ut.Message("R           reset the simulation", true);
        Ut.Message("Up, Down    change particle generation rate", true);
        Ut.Message("F1          print help", true);
        Ut.Message("Esc         quit the program", true);
        Ut.Message("Mouse.left  Trackball rotation", true);
        Ut.Message("Mouse.wheel zoom in/out", true);
        break;

        case Key.Left:
                DebuggerClass.ChangeIndex(true);
                // select previous launcher
                Ut.MessageInvariant($"current launcher index: {DebuggerClass.currentIndex}");

                break;
        case Key.Right:
                DebuggerClass.ChangeIndex(false);
                // select next launcher
                Ut.MessageInvariant($"current launcher index: {DebuggerClass.currentIndex}");
                break;

            case Key.Space:
                // user fires selected launcher
                DebuggerClass.globalLaunchers[DebuggerClass.currentIndex].Fire();
                break;

        case Key.Escape:
        // Close the application.
        window?.Close();
        break;
    }
  }

  private static void KeyUp(IKeyboard arg1, Key arg2, int arg3)
  {
    if (tb != null &&
        tb.KeyUp(arg1, arg2, arg3))
      return;

    switch (arg2)
    {
      case Key.ShiftLeft:
      case Key.ShiftRight:
        shiftDown--;
        break;

      case Key.ControlLeft:
      case Key.ControlRight:
        ctrlDown--;
        break;
    }
  }

  private static float currentX = 0.0f;

  private static float currentY = 0.0f;

  private static bool dragging = false;

  private static void MouseDown(IMouse mouse, MouseButton btn)
  {
    if (tb != null)
      tb.MouseDown(mouse, btn);

    if (btn == MouseButton.Right)
    {
      Ut.MessageInvariant($"Right button down: {mouse.Position}");

      // Start dragging.
      dragging = true;
      currentX = mouse.Position.X;
      currentY = mouse.Position.Y;
    }
  }

  private static void MouseUp(IMouse mouse, MouseButton btn)
  {
    if (tb != null)
      tb.MouseUp(mouse, btn);

    if (btn == MouseButton.Right)
    {
      Ut.MessageInvariant($"Right button up: {mouse.Position}");

      // Stop dragging.
      dragging = false;
    }
  }

  private static void MouseMove(IMouse mouse, System.Numerics.Vector2 xy)
  {
    if (tb != null)
      tb.MouseMove(mouse, xy);

    if (mouse.IsButtonPressed(MouseButton.Right))
    {
      Ut.MessageInvariant($"Mouse drag: {xy}");
    }

    // Object dragging.
    if (dragging)
    {
      float newX = mouse.Position.X;
      float newY = mouse.Position.Y;

      if (newX != currentX || newY != currentY)
      {
        //if (Objects.Count > 0)
        //{
        //  LastObject.Translate(new((newX - currentX) * mouseCx, (newY - currentY) * mouseCy, 0.0f));
        //}

        currentX = newX;
        currentY = newY;
      }
    }
  }

  private static void MouseDoubleClick(IMouse mouse, MouseButton btn, System.Numerics.Vector2 xy)
  {
    if (btn == MouseButton.Right)
    {
      Ut.Message("Closed by double-click.", true);
      window?.Close();
    }
  }


  private static void MouseScroll(IMouse mouse, ScrollWheel wheel)
  {
    if (tb != null)
    {
            tb.MouseWheel(mouse, wheel);
            SetWindowTitle();
    }

    Ut.MessageInvariant($"Mouse scroll: {wheel.Y}");
  }
}
