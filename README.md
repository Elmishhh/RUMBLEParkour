# RUMBLEParkour
 a text-file based parkour mod for RUMBLE vr

# creating maps

## initial map info and restrictions is down below, please replace the <> and everything inside with your map info, DO NOT USE : INSIDE  OF ANY OF THESE

map name:<replace this with map name here>
map creator:<replace this with map creator name here>
disallowed moves:<replace this with the moves you want to disable>
disallowed shiftstones:<replace this with the shiftstones you want to disable if they're requiped when you start the parkour>

## disallowed move names:
>            "explode",
>            "flick",
>            "parry",
>            "hold",
>            "dash",
>            "cube",
>            "uppercut",
>            "jump",
>            "wall",
>            "kick",
>            "stomp",
>            "ball",
>            "disc",
>            "straight",
>            "pillar",
>            "sprint" (yes you can disable sprint if you want i think)

## disallowed shiftstone names
>            "Vigor",
>            "Guard",
>            "Flow",
>            "Stubborn",
>            "Charge",
>            "Volatile",
>            "Surge",
>            "Adamant"

## the words before the : dont matter but please keep the : there

## now we move to objects, DO NOT HAVE SPACES HERE, IT MIGHT BREAK THINGS, | acts as a seperator between the info parts

objScaleX,objScaleY,objScaleZ|objPosX,objPosY,objPosZ|objRotationX,objRotationY,objRotationZ|ColorR,ColorG,ColorB,ColorA

## these values stand for the object Scale, Position, Rotation and Color 
## rotation values are 0-360 and the color values are 0-255 going by RGBA

## now lets move to modifiers, these come after your object info and are seperated by a \ (both \ and | should be located about your ENTER button, one requiring SHIFT to write)
## here are examples of modifiers and objects:

2,0.5,2|-25.6992,5.7348,-4.9633|0,0,0|255,0,0,255\Checkpoint:0,0,0
2,0.5,2|-27.6992,5.7348,-4.9633|0,0,0|0,255,0,255\Timed:3,3
2,0.5,2|-29.6992,5.7348,-4.9633|0,0,0|0,0,255,255\KillOnContact
2,0.5,2|-31.6992,5.7348,-4.9633|0,0,0|0,255,255,125\physics:1

## you can have multiple modifiers like:

2,0.5,2|-25.6992,5.7348,-4.9633|0,0,0|255,0,0,255\Checkpoint:0,0,0\Timed:3:3

## each modifier does something different,

> "Checkpoint:rotationX,rotationY,rotationZ" will set your checkpoint, recommended to be placed at the start, pressing right trigger in and touching an object with KillOnContact will instantly take you back to your last checkpoint

> "KillOnContact" it's what the name suggests, you touch it and you get sent to your last checkpoint

> "Timed:respawnTime,despawnTime" will make the platform temporarily disappear, respawn time is how long it takes to reappear and despawn time is how long it takes to disappear after being stepped on, respawnTime and despawnTime are integers so you cannot go into float points (you cant have the 0.5 in 2.5)

> "physics:bounciness" changes if the object will bounce you up when you land on it, 0 isnt bouncy and 1 retains 100% of the momentum(according to unity)

## none of the modifiers are capital sensitive but they are name sensitive so dont misspell them
## for examples of everything please look through other maps, ty for reading