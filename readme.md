

# Leaf Physics with Unity Jobs System

![](https://media.giphy.com/media/v1.Y2lkPTc5MGI3NjExOWYzNjAwNjYwMjVkNmFkZWZlYjRmZjYyMjIxOTI5ODdmY2Y4NTdiZiZjdD1n/2ptDU0fUNvVxxnN1wW/giphy.gif)

https://www.youtube.com/watch?v=iPJCbVxPcbY

This repository contains a powerful and efficient physics solution for Unity that uses the Jobs System for physics calculations. It is particularly useful for optimizing the performance of your project when rendering a large number of instances with physics interactions.

The core of this solution is the GPUInstancing class, which manages the rendering and physics calculations for instances. The class supports both traditional Update() calculations as well as Unity Jobs System-based calculations, which can be toggled with the useJobs boolean. The Jobs System can provide significant performance improvements by utilizing multi-threading and Burst compilation

# Features:

Efficient rendering of a large number of instances using GPU Instancing
Physics calculations using Unity's Jobs System for better performance
A custom PhysicsJob struct with BurstCompile attribute for faster execution
Use of the Material Property Block for efficient rendering with shadows
Support for both traditional Update() and Unity Jobs System calculations
Simple and customizable implementation
To use the physics solution in your project, simply add the GPUInstancing script to a GameObject, configure the properties as needed, and provide a mesh and material for rendering. Instances will be rendered and interact with physics based on the provided settings.



