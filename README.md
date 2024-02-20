## biocrowds-techdemo

# Overview
This package implements a variation of the BioCrowds Crowd Simulation model in Unity3D using a Data Oriented Approach.
The original BioCrowds is built on the idea that the simulation world can be split into discrete parts, which agents can compete for.
These parts form a Voronoi partition of the space, and agents capture the cells closest to them, and move only inside those cells.
This avoids collsion.

Two implementation issues that occurs from that idea, and the original algorithm are: 
- the discretized world data can become huge;
- the distribution of space among the agents needs special care when parallellizing the algorithm.

The original BioCrowds further partition space in a regular square grid, each cell containing some quantity of 'space markers'. These space markers are sample in the simulation initialization. This fixes the world size to a pre-defined region (where there are markers).

In this implementation, I changed the algorithm slightly to favor the data oriented simulation flow.
Instead of pre-sampling a partition of space, I compute the samples on the fly, during the simulation.
To compute the agent step, we sample new markers for each cell (and their neighbors) containing at least an agent, and distribute them among those agents.
This process comes with two immediate advantages, there is no data to fetch to compute the agent step, and the world can be arbitrarily large (as long as you can index each position in space.)
    
The current implementation does not parallellize the marker sampling and distribution step, and this is currently one of the goals of this project.

All that said, this package heavily uses Unity Native Collections, Parallell Jobs and Burst Mathematics to increase performance of the simulation.

    // Gifs
    // Images

# Requirements and Installation
This package was tested in _Unity3D 2022.3.1f1(LTS)_ and requires:
- Unity Burst (tested with 1.8.4)
- Unity Collections (tested with 1.2.4)
Then simply import the package.
 
## Simulation
You can initialize a simulation with the CrowdManager class.
Example:
     
```
git status
git add
git commit
```

     The package comes with an example component.

## Rendering
    The package comes with two example Renderers for the simulation.
    - The GPU instanced drawing CrowdIndirectRenderer
        (This uses a base mesh and Graphics.DrawMeshInstancedIndirect to draw the meshes).
    - The Unity Object based CrowdObjectRenderer
        (This one just positions game objects according to the simulation.)

    The example scene shows both of those working solutions.

# Implementation details


# Backlog
    - Parallellize the space distribution job.
    - Create a customizeable agent behavior system. (e.g. a 'script'-language)
    - Change the agent step data structure to a NativeArray instead of a NativeMultiHashMap to improve cache coherency.
