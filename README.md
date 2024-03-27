# This Repository Is Outdated
The new repository is [available here](https://github.com/vercidium-patreon/meshing), which features faster greedy meshing and a standalone renderer.

---
---
---

# voxel-mesh-generation
Optimised voxel mesh generation, written in C# and open sourced from the game [Sector's Edge](https://sectorsedge.com).


## Overview
This repository contains C# code for mesh generation on the CPU, setting up VBO and VAOs for OpenGL rendering, as well as fragment and vertex shaders.

The explanation and visualisation of this mesh generation technique can be found in [this blog post](https://vercidium.com/blog/voxel-world-optimisations/). Further optimisation details can be found in [this blog post](https://vercidium.com/blog/p/3b429724-2237-4bbd-88aa-af327a7d6ebb/).

Mesh generation and VBO buffering is kept separate so that multiple chunks can be initialised on different threads.

## Details
- Chunks are composed of 32x32x32 blocks
- Faces are generated by combining faces in runs along the X and Y axis.
- This algorithm produces \~20% more triangles than greedy meshing and runs \~390% faster
- Block position, texture ID, health and normal are packed into 4 bytes for faster buffering and rendering on the GPU.

## Benchmarks
Benchmarks were run with a Ryzen 5 1600 CPU

- ST = Single Threaded Time
- MTT = Multithreaded Time - 12 threads each initialising 1/12th of the map
- CMGT = Chunk Mesh Generation Time

| Map Name          | Chunk Count | Total STT (ms) | Total MTT (ms) | Average CMGT (ms) |
|:-----------------:|:-----------:|:--------------:|:--------------:|:-----------------:|
| Crashed Freighter | 799         | 421            | 110            | 0.527             |
| Soltrium Temple   | 648         | 280            | 73             | 0.432             |
| Aegis Desert      | 441         | 201            | 51             | 0.456             |
| Magma Chamber     | 293         | 157            | 38             | 0.535             |
| Arena             | 108         | 30             | 12             | 0.278             |

## In Practice
With 16 players per match, multiple chunk meshes must be regenerated every frame. This process has been optimised to ensure the game can be played smoothly on older hardware.

![Soltrium Temple Screenshot](https://vercidium.com/blog/content/images/size/w2000/2020/01/cover-1.jpg)
