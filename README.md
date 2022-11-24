## Tennis example project

Usage: Start 2 Instances, one in unity editor, host with the unity editor instance (for game messages) and be a client with the 2nd one.

### Controls: 
- WASD - Move character (while serving or not having control of the ball)
- Mouse - Move camera (also changes direction of shot)
- Left Click - Shoot (Only if you have the ball)
- Q, R (as Host) - Add a random point, reset game

During serve you must hit the corresponding service box (no second try logic for serves), to intercept the ball just make the character collide with it and then aim and shoot the ball back with left click.

3 sets of 7 games are played (the match also ends if first two are won by the same player)
