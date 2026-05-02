# Iterations:

mlagents-learn "results\Andreea_TrainingI_v2\configuration.yaml" --run-id=Andreea_TrainingI_v2 --resume

.\venv_310\Scripts\activate

## Iteration I

### Observations (13)
- **Direction from dog to Sheep** (Vector3): Dog-to-sheep vector from the dog position
- **Direction from Sheep to Pen** (Vector3): Sheep-to-pen vector in global space.
- **Distance from dog to Sheep** (float): The normalized distance between the dog and the sheep
- **Distance from Sheep to Pen** (float): The normalized distance between the sheep and the pen goal
- **Dog Alignment with Sheep** (float): A Dot Product indicating whether the dog is facing the sheep
- **Dog Velocity** (Vector3): The dog's current linear velocity normalized by its movement speed
- **Alignment Goal** (float): A Dot Product indicating how well the dog, the sheep, and the pen are lined up

### Reward System

Positive Rewards:
- **Goal Success** (+100.0): Granted immediately when the sheep reaches the pen
- **Herding Position Alignment** (+0.005 per step): Awarded if the dog is close to the sheep (<2 units) and perfectly lined up behind it relative to the pen goal
- **Approaching the Sheep** (+0.001 per step): Awarded whenever the dog decreases its distance to the sheep.
- **Proximity to Sheep** (+0.002 per step): Awarded when the dog maintains a close distance (<1.5 units) to the sheep
- **Pushing Sheep to Pen** (+0.1 per step): Awarded when the sheep moves closer to the pen goal

Negative Rewards:
- **Moving Sheep Away from Pen** (-0.05 per step): Penalizes if the sheep moves away from the goal.
- **Unnecessary Rotation** (-0.002 per step): Penalizes the dog for rotating/turning when it is already directly facing the sheep
- **Time Penalty** (-0.001 per step): A small penalty applied at every single step
- **Episode Timeout** (-1.0): Applied if the agent exceeds **4000** steps without reaching the goal, resetting the episode

### Issues:
- Overfitting: While the agent successfully learned the task for a fixed environment layout, it failed to generalize when the pen was relocated
- The agent learned to exploit these small rewards and circled behind the sheep

## Iteration II- v13

### Key Improvements:
- Testing environment: randomizes the agent, sheep, and pen positions every 10 episodes, increasing the gap from 10
- Random Initial Rotation: The agent spawns with a randomized y-axis rotation
- Local Space Observations: All vector observations (directions to sheep/pen, velocity) are converted from global coordinates to the agent's local space using transform.InverseTransformDirection

### Observations (13)
- **Local Direction from dog to Sheep** (Vector3): Dog-to-sheep vector in the dog's local space
- **Local Direction from Sheep to Pen** (Vector3): Sheep-to-pen vector in the dog's local space.
- **Normalized distance from dog to Sheep** (float): The normalized distance between the dog and the sheep
- **Normalized distance from Sheep to Pen** (float): The normalized distance between the sheep and the pen goal
- **Dog Alignment with Sheep** (float): A Dot Product indicating whether the dog is facing the sheep
- **Local dog Velocity** (Vector3): The dog's current linear velocity normalized by its movement speed
- **Alignment Goal** (float): A Dot Product indicating how well the dog, the sheep, and the pen are lined up

### Reward System

Positive Rewards:
- **Goal Success** (+100.0): Granted immediately when the sheep reaches the pen
- **Herding Position Alignment** (+0.005 per step): Awarded if the dog is close to the sheep (<13 units) and perfectly lined up behind it relative to the pen goal
- **Dynamic Distance Reward** (Continuous): Calculated via distanceDelta * 0.02f. It scales the reward directly to how much the dog closes the distance to the sheep (changed from **Approaching the Sheep**)
- **Proximity to Sheep** (+0.002 per step): Awarded when the dog maintains a close distance (<6 units) to the sheep
- **Pushing Sheep to Pen** (+0.1 per step): Awarded when the sheep moves closer to the pen goal

Negative Rewards:
- **Dynamic Distance Penalty** (Continuous): The **Dynamic Distance Reward** becomes penalty if the dog moves further away from the sheep

- **Moving Sheep Away from Pen** (-0.05 per step): Penalizes if the sheep moves away from the goal.
- **Unnecessary Rotation** (-0.002 per step): Penalizes the dog for rotating/turning when it is already directly facing the sheep
- **Time Penalty** (-0.001 per step): A small penalty applied at every single step
- **Episode Timeout** (-1.0): Applied if the agent exceeds **4000** steps without reaching the goal, resetting the episode

### Results:
- It fixed the overfitting
- The agent is not good with steering the sheep in the right/left and it ends up steering the sheep in a few circles before reaching the pen

## Iteration III- v15

### Key Improvements:
- Ideal Steering Spot: Instead of just moving toward the sheep, the reward system now guides the agent to a specific point behind the sheep
- Directional Cross Product Observation: Added a Vector3.Cross calculation to the observation vector (cross.y). This explicitly tells the agent if the sheep is to the left or right of the line connecting it to the goal

### Observations (14)
- **Local Direction from dog to Sheep** (Vector3): Dog-to-sheep vector in the dog's local space
- **Local Direction from Sheep to Pen** (Vector3): Sheep-to-pen vector in the dog's local space
- **Cross Product Y-Axis** (float - 1 value): Measures Vector3.Cross(dirToSheep, dirSheepToPen).y - tells the dog how to circle to get behind the sheep
- **Normalized distance from dog to Sheep** (float): The normalized distance between the dog and the sheep
- **Normalized distance from Sheep to Pen** (float): The normalized distance between the sheep and the pen goal
- **Dog Alignment with Sheep** (float): A Dot Product indicating whether the dog is facing the sheep
- **Local dog Velocity** (Vector3): The dog's current linear velocity normalized by its movement speed
- **Alignment Goal** (float): A Dot Product indicating how well the dog, the sheep, and the pen are lined up

### Reward System

Positive Rewards:
- **Goal Success** (+100.0): Granted immediately when the sheep reaches the pen
- **Orbital Position Reward** (+0.001 per step): Awarded when the dog gets close to the ideal space behind the sheep (<5.0 units away)
- **Direct Drive Alignment** (+0.005 per step): Awarded when the dog is in the ideal space and its body is facing the pen goal
- **Herding Position Alignment** (+0.005 per step): Awarded if the dog is close to the sheep (<13 units) and perfectly lined up behind it relative to the pen goal
- **Dynamic Distance Reward** (Continuous): Calculated via distanceDelta * 0.02f. It scales the reward directly to how much the dog closes the distance to the sheep
- **Pushing Sheep to Pen** (+0.1 per step): Awarded when the sheep moves closer to the pen goal

Negative Rewards:
- **Dynamic Distance Penalty** (Continuous): The **Dynamic Distance Reward** becomes penalty if the dog moves further away from the sheep
- **Moving Sheep Away from Pen** (-0.05 per step): Penalizes if the sheep moves away from the goal.
- **Time Penalty** (-0.001 per step): A small penalty applied at every single step
- **Episode Timeout** (-1.0): Applied if the agent exceeds **4000** steps without reaching the goal, resetting the episode

### Results:
- The agent can steer the sheep from any point to the pen effciently
