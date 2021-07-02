<p align="center">
  <img height="300" src="https://user-images.githubusercontent.com/8492408/124276070-a6a42880-db43-11eb-89af-4e612eb66620.png">
</p>

# About

A simple and fast Entity-Component-System for C#.

Every entity is represented as an int.

The code is self-documenting. Start with creating an instance of Context.

# How to use

1. Create an instance of Context.
2. Add custom Systems and Components.
3. Call Context.Update() every frame or use Context.SendEvent() to send custom events.

The example is in the Test directory.
