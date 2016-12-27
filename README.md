# About

Simple and fast Entity-Component-System for C#.

All operations: adding, removing, querying, and iterating have reasonable time complexities.

Every entity is represented as an int.

The code is self-documenting. Start with creating an instance of Context.

# Usage

1. Create an instance of Context.
2. Add custom Systems and Components.
3. Call Context.Update() every frame or use Context.SendEvent() to send custom events.

The example usage is in the Test directory.
