This is the documentation for the 08-Fireworks homework.

Disclaimer: this program was created by modifying the pilot project 08-Fireworks.sln.
Most changes are in the `Particle` and `Simulation` classes, the rest is mostly left untouched.

I created a few other classes to enable the user to modify and customize the fireworks simulation easily.
Here is a breakdown of what those classes and their purpose:
`ParticleType`
    This class is used to define a template for the `Particle` class. Here, the user can define a type of particle that has certain characteristics:
- color
- size
- mass
- maximum age
- drag
- type of explosion (for example: how many particles of which type will be generated in the explosion? described in detail below)

`Explosion`
Instances of this class define different types of explosions: the user can choose the following: 
- number of new particles created in the explosion
- strength of  the explosion ("how far will the particles fly")
- explosion strength randomness (how much variance there will be in how far the individual particles fly, calculated from the explosion strength)
- different types of the particles generated by the explosion (passed to the constructor as a list)
There is a special instance of this class, `EmptyExplosion`, that doesn't generate any particles at all.

`Launcher`
Instances of this class generated new particles periodically. The user can choose these characteristics:
- position of the launcher
- interval of particle launches
- number of particles launched at once per interval
- particle launch strength
- launched particle type

`CometTail`
This class is used for creating templates for the look of the tail/trajectory that particles leave behind.
The user can define these characteristics:
- once in how many frames will a tail particle be generated (the density of the tail; recommended value is 1, otherwise the tail doesn't look good)
- which type will be the the tail particles (tail length will be affected by the age given by the ParticleType instance)
    - higher than usual drag coefficients are desirable for the particle type: this will make the trajectory more stationary and look more realistic
There is a special instance of this class, `NoTail`, that should be assigned to particles aren't supposed to have a tail.

**Animation of launch ramps**
I decided to visualize the invisible launchers by yet another launcher. This launcher is stored in the `fireSparklesLauncher` variable. It shoots out a large number of orange particles with a short lifespan to imitate the appearance of sparkles shooting out of a lit firecracker.

**Multiple rocket/particle types**
As I've described above, the user can design a new type of particle and modify any of its parameters/fields. The same applies for launchers, and tails/trajectories.

**Multi-stage explosions**
I've implemented a robust support for multi-stage explosions – in theory, there is no limit as to how many stages an explosion can have. For example, the user can specify that `explosionA` generates particles `particleA`, which in turn explode in `explosionB` that generates `particleB` , and so on. This chain can go on as long as the user desires; however, a maximum of a three-stage explosion is recommended for performance reasons.

**Color changes during life of a particle**
I've implemeted only a very simple feature, where the particle is white when launched and then changes gradually to its right color as it slows down. This is meant to convey that the particle is extremely hot at the beginning and then loses some of its temperature as it slows down. This feature can be turned on/off by setting the bool field `speedColor` of the struct `particleClass` while creating a new instance of this struct.

**Visualization of rocket trajectories**
There is a robust support for visualizing the particle's trajectory using other particles: the user can define an instance of the `CometTail` class, and then assign it to an instance of `ParticleType`. Particles with a tail will leave behind tail particles, which are can be defined the same way as any other particle. Thus, the user has complete control over how the tail will look like (e.g. if the tail particles will be moved by gravity, how long their lifespan will be, how often will a tail particle will be generated, etc.)

**Interactive fireworks control**
- the user can make a selected launcher shoot out a particle by pressing the `space` button
- selected launcher can be changed by pressing the left and right arrow keys, and the index of the new launcher will appear in the console


I **have not** implemented any shading effects.


**Use of AI assistant**
I had problems finding values of the linear and quadratic coefficients that would make the particles' trajectories look natural, so I used ChatGPT to suggest those values. It improved the appearance somewhat, but it's still not perfect. In fact, I couldn't get it to look very realistic without changing gravity.


**Object Hierarchy**
There's a tree structure in the way how each object (launcher, particle, explosion, tail) has other objects assigned to it.
Here's a template for the basic structure of the tree:

```
launcherA
    particleA
        explosionA
            particleA1
                EmptyExplosion
                NoTail
            particleA2
                EmptyExplosion
                NoTail
        NoTail
```

As an example, here's a tree tree of an actual launcher from my program:
```
bigDoubleExplosiveLauncher
    DoubleExplosive
        explosiveParticleExplosion
            longExplosiveBlue
                complementaryBluePink
                    smallGreen2Nonexplosive
                        EmptyExplosion
                        NoTail
                    smallPinkNonexplosive
                        EmptyExplosion
                        NoTail
                NoTail
        NoTail
```