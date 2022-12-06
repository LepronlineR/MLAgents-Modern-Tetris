# MLAgents for Modern Tetris

## What is Tetris?

Tetris is a simple grid like game, where the player move different shaped pieces, with movement and rotation. These pieces in the board will slowly fall down to the screen, in which will eventually stack up together. However, if a row fills up with the pieces, then it will dissapear. The pieces are spawned on the top of the board, descending to the bottom of the board every X second(s), if the piece is spawned on the board and there are no valid moves, i.e. the player cannot move that piece, then the game is over.

![TetrisGame](ProjectFiles/tetris.gif)


## Optimization



References
[1] https://codemyroad.wordpress.com/2013/04/14/tetris-ai-the-near-perfect-player/

## Version

This project uses Unity (2021.3 or later) and Python (3.8.13 or higher)

## Instructions

Creating a virtual environment with the project (to run MLagents)

1. cd to the game directory (i.e. Windows: C:\Users\ ...\ ...\MLAgents-Modern-Tetris)

2. setup a directory \<dir-name> as the directory and location of a new virtual environment
- python -m \<dir-name> venv

3. cd to the directory and activate the virtual environment
- cd \<dir-name>\Scripts -> activate

3. upgrade pip (for venv)
- python -m pip install --upgrade pip

4. install pytorch (for venv)
- pip3 install torch~=1.7.1 -f https://download.pytorch.org/whl/torch_stable.html

5. install mlagents (for venv)
- python -m pip install mlagents==0.30.0

## MlAgents Misc

To view a description of all the CLI options accepted by mlagents-learn, use the --help:
mlagents-learn --help

The basic command for training is:
mlagents-learn \<trainer-config-file> --env=<env_name> --run-id=\<run-identifier>
i.e.) mlagents-learn config/agent-config.yaml --run-id TetrisPPO --force