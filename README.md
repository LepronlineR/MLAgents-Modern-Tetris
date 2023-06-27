# Report: RL for Tetris

## What is Tetris?

Tetris is a simple grid like game, where the player move different shaped pieces, called <b> tetrominos</b>, with movement and rotation. These pieces in the board will slowly fall down to the screen, in which will eventually stack up together. However, if a row fills up with the pieces, then it will dissapear. The pieces are spawned on the top of the board, descending to the bottom of the board every X second(s), if the piece is spawned on the board and there are no valid moves, i.e. the player cannot move that piece, then the game is over.

![TetrisGame](ProjectFiles/tetris.gif)

## Motivation

Tetris is a competitive game, with players constantly updating how the game is played at all times. Many RL methods like Q-learning could solve the original NES tetris game, which is very old. Current standards of tetris feels and acts differently from the past tetris games, and thus we can consider them different games. Take for example, the scoring, in old tetris your score is equivalent to the lines you clear, thus you only gain a fixed ratio for the total lines you clear, where in the new tetris, you have different scoring methods like the T-spin (where you have to rotate a T-tetromino inbetween at 3 corners while clearing X lines). Thus, my goal is to revive the Tetris AI to a much newer standard of playing, and hopefully can learn the newer skills in modern tetris that old tetris never considers. 

### Why Machine Learning?

We can first confirm that tetris is a problem that is NP complete. We can easily look at if there is a solution for a piece in some kind of board state. This board state can be new or even contain many different stacked pieces. However, if we intend to maximize the amount of scores that we can achieve, this means that there are multiple combinations that require many different pieces in some kind of order to achieve that.

We can prove that this problem is NP-hard by reducing it from another problem: 3-Partition. The proof is given under the resources [1] and I will not extensively delve into why. Nonetheless, with this proof we know that the game is deterministic. However, still with many variables to consider, as well as the many possibilities to play the game, a large state space, and what problems we could solve before moving onto larger ones. With that many considerations, we can look to reinforcement learning in order to solve this problem.

Note that this project is really hard to work on in Colab or Jupyter notebook due to the fact that it has a limited runtime, whereas hard RL problems need a lot of time to train.

Resources:
[1] https://erikdemaine.org/papers/Tetris_TR2002/paper.pdf

## Standardization of Tetris

As a game that has been around for almost 40 years, there has been multiple versions of the game that alter the rules of Tetris. Therefore, a standardized rule/guideline was made in the early 2000s to generalize the Tetris game. Each rule/game mechanic will be explained further in detail in the implementation..

In our version, we chose to implement most of the ruleset, while leaving out some, the included rules and gameplay are included as follows (✅ Included, ❌ Ignored/Removed/Not Implemented)
(Note: Although there are strict definitions for certain rules, those are not includeded i.e. where the tetrominos start)

* ✅ A 10x20 Board with all 7 Default Tetrominos (I, O, T, S, Z, J, L)

* ✅ Implements the SRS (Super Rotation System)

* ✅ Standard Controls (Rotate Left, Rotate Right, Move Left, Move Right, Move Down)

* ❌ Hard Drop (instant dropping a tetromino)

* ✅ Tetromino Randomizer (7-Bag system) -> Spawning the tetromino piece

* ❌ "Hold Piece": can choose to hold a piece until swapped out with another piece to replace in the play boad

* ❌ "Future Pieces": you can see the future pieces (after) your current piece

* ✅ "Ghost Piece" (Disabled in training) -> shows where the piece will land

* ✅ T-Spins: by rotating a t-piece in a specific location (with 3 corners)

* ✅ Lock delay: if you land a piece, there is some time (half a second) to move/rotate a piece

* ✅ Combos: by clearing lines consecutively, you are able to gain more subsquent combos that will earn more points

* ✅ Back to back: by accomplishing a "difficult" move, you earn more points (Examples of difficult moves: T-spins, Tetris (4 lines cleared at once), Perfect Clear)

* ✅ Perfect Clear: if you clear the entire board you are able to earn more points

References
[1] Rules and Guidelines: https://tetris.fandom.com/wiki/Tetris_Guideline

## Tetris Game

In our project, the game is implemented in Unity and C#. However, the logic still remains the same for the pseudocode. The environment takes one value (the action) and returns three values: the rewards given the reward schedule, the observation from the environment.
```
def logic():
    while(true):
        reward, obs = env(action)
        
        # AI operations
```
The reward schedule is variant, and there are multiple reasons to include different reward schedules.

Here is the current <b> reward schedule </b>:

- (penalty) -5 points for losing
- for each block that lands 0.005 * multiplier, where multiplier = how low/high a block is 
- scoring squared (clearing lines, t-spin, etc), which are normal scoring methods in traditional tetris, but the results are squared

Our reward schedule wants to incentivise the AI to continue placing blocks as low as possible, while penalizing them for losing. To keep true to tetris scoring, we have that as a reward schedule, this potentially should yield higher returns when a score is made with that metric. Furthermore, by scoring more lines, since the results are squared, the higher rewards should be present.

The agent/player has several <b> actions </b> they can take: moving down, moving left, moving right or rotating left, rotating right. Of course in these decisions, the AI also has the ability to take no action. Here we relegate these discrete action spaces to two different results, since we want the agent to actually move and rotate at the same time in order to make effective movement decisions. However, this might be a red herring and possibly maybe even sabotage our agent behavior (since you might see at the end the agent moves sporadically).

Naively, we can assume that our observation is the screen space, which is just a vector space of the board. If there is a piece at that location it would be 1 and 0 if none. The <b> Environment </b> contains a 10 x 20 board, in which pieces will start at the top and slowly descend to the bottom. Each piece willlock in place once they hit a bottom platform of any kind.

I chose to disable hard dropping in the game because it has a chance to sabotage any AI in learning where to drop in its exploring step, if the AI chooses to drop the piece and learns to hard drop almost every time, then it might take substantially much longer in order to learn where to drop the piece, thus wasting training time. Since we just want it to play modern tetris effectively we can introduce this variable in after we know the approximate time that it will learn to play the game.

##### Randomizer

In classic tetris there is a problem called tetrominos drought, that is when you dont get any tetrominos that you need as well as piece flood, where you get too many bad tetrominos in a row. To solve this problem there is a randomizer called the 7 bag randomizer, which shuffles 7 tetrominos in a bag and then those tetrominos will be the next 7 tetrominos. Then if the last tetromino is used, a new 7 bag will be generated. This prevents the prior issues that are brought up.

#### How does spinning work?

Tetris introduces a system called locking, when the piece falls to the ground it has a small time frame in which you can rotate or move the piece before it locks in place and a new piece will fall down. This actually gives the opportunity for us to rotate the piece in some way. Introducing the T-spin, when you have X clearable rows and with 3 corners being occupied with where you drop the last piece, and rotating that t piece to clear the rows, you effectively have done a t-spin, which is generally a hard maneuver in tetris. Here is a small gif of how it is done.

![tspin](ProjectFiles/tspinshow.gif) 

#### How does full clearing work?

When you clear the entire board with all the pieces, you achieve a full clear, which gives you many new additional points. This comes with a lot of planning so it is an advanced move.

#### What are combos?

When you clear any lines in consecutive order without placing a block without clearing a line, you begin a combo, if you continuously add blocks while clearing you add more to the combo and get additional points for the difficulty of the line cleared.

#### What is Back to Back?

In tetris, there are difficult manuevers, which are the t-spins, full clearing, and getting a tetris (clearing 4 lines in one go). These manuevers are rewarded by adding an extra multiplier called back to back. This incentivizes players to take more risks and perform more difficult moves.

References:
[1] T-spins: https://aminoapps.com/c/puyo-puyo-tetris/page/blog/t-spin-tutorial/JaZ1_ZJidurXexwY56lXlbbbm3ERwavXBp

## Optimization

When you consider a human playing tetris, an observation at the specific cells would not yield as high results than looking at the possibility of the next placed piece in some location. Thus, we can further optimize the observation, from looking at the screen, we should look at the next potential moves that we could make, and from that derive a metric to look at those observations.

Assuming that a board is static for a single observation, if we drop a piece in every position as well as for every rotation, we can observe the results from that drop. How should we observe that result? Here are 4 heuristics to consider: the aggregate height, any complete lines, all holes, and the bumpiness.

- <b>Aggregate Height</b>: this value represents the sum of all the heights in the board, if we consider a high board (which has higher pieces), this value will generally be greater. Thus, the AI's goal should be to minimize this value.

![AggregateHeight](ProjectFiles/121.png) 

- <b>Complete Lines</b>: this represents the total amount of lines that would be cleared. This value should be maximized for greater rewards.

![CL](ProjectFiles/2.png) 

- <b>Bumpiness</b>: if we compare every column next to each other, some taller columns risks the structure being much taller than what it should be. Given that, the bumpiness is the difference between each column amount summed. 

![bump](ProjectFiles/121.png) 

- <b>Holes</b>: if we play enough tetris, we know that holes can be a nightmare, since they prevent lines from being cleared at an optimal pace, as well as poised to leading to a game over. Therefore, if we have the option to we want to have the minimum amount of holes.

![holes](ProjectFiles/3.png) 

### Adaptation

Thus, for each current tetris piece, we look at each x position (from 0 to 10) and every rotation for that piece (4 rotations). Then calculate the heuristics for each, which results with 4 * 4 * 10 = 160 total observations for the possible choices. +1 for an observation of the current tetromino. 

References
[1] Four Heurisitics for Tetris: https://codemyroad.wordpress.com/2013/04/14/tetris-ai-the-near-perfect-player/

## Models

### Model Overview

![PPO](ProjectFiles/TetrisPPO.onnx.png) 

- Model: Recurrent with 128 hidden units and 2 layers

### Proximal Policy Optimization (PPO)

There are many different models and architectures to consider with how advanced RL methods have come. For our game, we prefer an architecture that can avoid large policy updates, which is what PPO excels at.

Proximal Policy Optimization, or PPO, is a policy gradient method for RL that optimizes a "surrogate" objective function.

- When we have a policy update that takes a step in the wrong direction, which we consider a bad policy, it can be hard to recover from that loss. Thus, we want to alleviate this problem by updating the policy liberally. We get these values by calculating the ratio between the last policy and the current policy, and result in a range [1 - epsilon, 1 + epsilon], this is the <b>Proximal Policy</b>.

- Ratio:

    $${\pi_\theta(a_t|s_t) \over \pi_{\theta_{old}}(a_t|s_t)}$$
    
    - divergence between old and current policy 

        - if ratio > 1 : action and state = current policy
        - if 0 < ratio < 1 : action and state = old policy

- Previous Policy Objective Function (REINFORCE): 
    $$L = \hat {\mathbb{E}}_t [log\pi(a|s) * \hat A_t]$$

By doing a gradient ascent our agents should take actions that lead to higher rewards. However, since we have this idea of clipping, we adapt it to the policy update.

- Surrogate Objective Function: 

    $$L^{CLIP}(\theta)=\hat {\mathbb{E}}_t \left [ \min(r_t(\theta) \hat A_t, \text{clip}(r_t(\theta), 1-\epsilon, 1+\epsilon)\hat A_t) \right ]$$

    - Clipping *clip* in the objective function, to prevent large policy updates

```
# Here is a pseudocode example of the policy update
# policy update
def policy_update(self, model):
    # evaluate the policy
    pi = policy.evaluate()

    # find the ratio
    ratio = pi / pi_old

    # surrogate loss
    loss = min(ratio * advantage, clip(ratio, 1 - eps, 1 + eps) * advantage)

    # update gradients
    update_gradients()

```

References
[1] PPO paper https://arxiv.org/pdf/1707.06347v2.pdf

### Inverse Reinforcement Learning

Reinforcement learning takes an environment and rewards to give us an action for the agent to effect the environment. However, we can think of the opposite, the inverse as such (inverse reinforcement learning). This takes the agent's objective and attempts to predict the object, value and rewards from the agent. From this we can use data in order to help train our reinforcement learning model. 

#### Generative Adversarial Imitation Learning (GAIL)

Modern tetris is being played frequently by players, and with the amount of difficult decisions that we make when playing modern tetris we could use help from demonstrations to learn a correct way to play tetris. Thus, we look at GAIL, a method that utilizes Generative Adversarial Networks in order to learn a more effective way to play tetris. 

GAIL is model free, and thus looks at actions from human demonstrations and derive a reward from the descriminator. Then the generator, which is PPO in our case, creates a policy.

We can utilize the expert opinion from a player "me" in order to train our AI with a better idea of how the game should be played. When you should consider dropping a piece when it is 1 height or if it is 4 heights for maximizing the points. 

The data from GAIL is recorded from me in a 1 hour playthrough of tetris in the environment. It is recorded as TetrisDemo.demo. I have achieved over 100 rewards in the playthrough, thus I consider myself an expert player in that demonstration.

References:
[1] https://arxiv.org/pdf/1606.03476.pdf

## Implementation

These models are completely generated through Unity's ML Agent API. In which, when the user gives the agent an environment and the reward signals, the agent is generated from a .YAML config file and set to train due to specific commands entered into the configurations. The config is in the config file. Thus, for our implementation we only can assume that our theory to solve this problem is correct, in which we are able to solve this problem, as well as the environment of tetris that is provided with the agent, including the rewards the agents recieve. The observation space is also calculated in the game, since it doesn't just naively look at each tile, it looks at every potential move that it can make and calculates it based on the heurisitcs. 

Here are some gifs of the AI while training:

![training1](ProjectFiles/training.gif) 
![training2](ProjectFiles/training2.gif) 

## Results

https://www.youtube.com/watch?v=-Cb9MtgNxUg

This model has been training for 5 million epochs, and for around 4 hours with 32 agents training at the same time. 
Here are the hyperparameters for the first model:

PPO:
- <b>Batch Size </b> (batches used for experience replay): 512
- <b>Buffer Size </b> (the size of the list containing all of the experience replay buffer): 409600
- <b>Learning Rate </b> (learning rate of the model, corresponds to the gradient descent in every step): 0.001
- <b>Beta </b> (entropy regularization -> ensures that the agent exploits more): 0.01
- <b>Epsilon </b> (threshold of divergence between policies): 0.3
- <b>Lambda </b> (used for Generalized Advantage Estimate): 0.95
- <b>Num Epoch </b> (the total amount of epochs passed to the experience replay buffer): 5
- <b>Sequence length </b> (the amount of epochs passed to the replay buffer) for a recurrent network: 64

GAIL:
- <b>Expert Data </b>: 60 minutes of gameplay
- <b>Strength </b>: (reward multiplier, but in short the % of us comparing the agent to our expert player as the expert player for the discriminator) 0.5
- <b>Gamma </b>: (discount factor) 0.99

Both models, PPO and GAIL, contains 2 layers with 128 hidden units per layer.

![reward](ProjectFiles/reward.png) 
![loss](ProjectFiles/loss.png) 
![gail](ProjectFiles/gail.png) 

![results](ProjectFiles/results.gif) 

## Discussion

As you can see the results are bad, somehow after 500,000 iterations, there is a steep decline in the cumulative reward. What could be causing this?

- If we examine the data, we can tell the the model slowly over time loses more average reward and is steadily declining over 5.5 million epochs. The GAIL loss decreases, while the model loss increases, which is opposite of what we want. Furthermore, another good data point to examine is the GAIL expert estimate, if there is a higher GAIL expert estimate, than it predicts the right expert player more frequently, which is in this case increasing, and not what we want.

- A more defined reward schedule: perhaps the penalty is too much, the agent could have learned to just not take that penalty, and instead "stall for time", which is take unnecessary actions to not lose the cumuluative reward over time. The agent could also know that the trade off for the reward is more if you continuously stack pieces than steadily place it in the right direction and placing the piece at that location.

- We can even consider taking a longer time to train our model. Most RL models take more than just 5 million epochs, or just more than 1 day itself to be an effective model. However, given the time constraint and how hard the problem is compared to the easier RL problems like lunar lander or cartpole, this model can take up to many days to even learn something effective. Even some simple Atari games struggle to learn effectively until over half a day of training.

- We might not even consider that at 500k iterations it is slowly not improving, perhaps we need to make the model train much much more slower, so that it can learn to explore more often. In this case, it is slowly declining since we might believe that the model will learn much faster, and will exploit as fast as possible. However, from the results we can tell that the agent did not learn much from taking these actions.

- From the NN at the end, it is taking a lot of unnecessary actions and thus takes a long time to lock the tetrominos in place. Thus perhaps, we can introduce a penalty for the amount of actions that a piece can take? This can be helpful or very dangerous. We have the side of the pieces not wanting to be rotated and then leading to no exploration for turning pieces since the agent might learn to avoid doing that. Furthermore, on the other end this can solve the issue of the network taking actions as much as possible.

- The last improvement that I could make is to give it more expert data, but since it is not learning much from the small data that it already recives, it seems like it will not improve the model by much if it starts training. If we look at the expert estimate for the discriminator, we can easily say that the discriminator is effectively predicting the expert player, as the generator is not performing that well. This aligns with our data and expectations, which should mean that this data is generally not the issue.

- Here is another issue to consider with the model as well, what if we assume that the model is learning effectively? Then we can also add onto the fact that the model might lose more reward over time, because they are exploring more effectively by dropping the pieces more, which should lead to them losing more rewards over time since it gets penalized when the game loses. This could mean that given more time the agent would be able to learn effectively, but not now.

## Second Try

I ran the model a second time with some parameters slightly changed in order to see some kind of repeating pattern in order to diagnose this issue more effectively. I chose to add more hidden units to my layer since there is a suggestion that the NN might need to make more advanced decisions and thus would require a larger model. This took me 6 million epochs, which is around the same time (4.5 hours), I wanted both of the models to have the same training time so that my comparison would be much better.

PPO:
- <b>Batch Size </b>: 512
- <b>Buffer Size </b>: 409600
- <b>Learning Rate </b>: 0.001
- <b>Beta </b>: 0.001
- <b>Epsilon </b>: 0.3
- <b>Lambda </b>: 0.95
- <b>Num Epoch </b>: 10
- <b>Sequence length </b>: 64

GAIL:
- <b>Expert Data </b>: 60 minutes of gameplay
- <b>Strength </b>: 0.5
- <b>Gamma </b>: 0.99

![2ndmodel](ProjectFiles/TetrisPPO2.png) 
![reward2](ProjectFiles/reward2.png) 
![loss2](ProjectFiles/loss2.png) 
![gail2](ProjectFiles/gail2.png) 

## Discussion 2

This model does clarify more about the problem, as we can see the spike did not effect any of the updates at all and the model still does not learn effectively, this means that the model is currently still in exploration at this point, and we still cannot consider it fully trained. Thus, if we give this model another week to train under the same circumstances, perhaps it will get much better over time. With how the model takes a lot of computer power (since it is from a laptop) it is effectively hard to train, as it is not fully using all the cores avalible to train.

## Conclusions

As one of the projects that did not actually solve the problem, especially in reinforcement learning, there is a lot to reflect from and possibly from the data, predict what could have gone wrong with the model. Since I did have a second attempt at solving the problem this means that my model does actually have the potential to solving the problem, but with the amount of power needed to computer with reinforcement learning it is harder to prove and harder to train. I still believe that my solution would work with this problem, but what is ahead could be the model playing tetris. Since we have previously trained tetris models to play using Q Learning, it is very possible that PPO with GAIL will solve this problem. However, the case still stands with the complexity of the problem, since we are adding more exteneous reward signals, such as ways to gain more points (t-spinning, perfect clears, etc), we want the models to eventually perform those tasks over just clearing one or two lines, since the trade off for those actions is effectively more rewards than previously. Unfortunately these observations will be ahead of the project (which will be completed on my own), since there is a lack of time as well as computing power to solve this problem at this moment.

## A year later... Fixing the "problems"

There are some ways that we can improve this model significantly. In order to see some more positive results, we may have to look at the reward schedule again. The current reward schedule is based on height, and not very well thought out. Therefore, if we can improve the results throughout training, we have to train it to play normally:

* learning to play tetris (stacking and placing)
* learning advanced techniques (t-spin, combos, etc) [later]

### Looking at the major problems

The first major problem was how we achieve the observation space:
```
// observations[0] = Current Tetromino Piece
// observations[1->160] = every rotation of a piece (4) * every position (10) * evaluations (4)
// evaluations:
//  - # of lines that will be cleared
//  - # of holes in the grid
//  - grid bump
//  - grid sum height
```
Here are the major problems with this:

- the observation does not really track every tetromino piece, all the pieces will perform different actions. We need to track every observation of every different piece, otherwise the network will update the piece differently due to how the action of that piece will go. Furthermore, we should have the agent observe the board, since the agent has no idea what the board looks like, and just the actions that the pieces can take, the agent most likely does not have a good idea on what actions it should take. Therefore, it is recommended to add the state of the board as an observation.

There are multiple solutions to this:

1. Here we just add all the rotations/movements/evaluations to every tetromino pieces.
``` 
// observations[0 to 1] = Current Tetromino Piece
// observations[1 to 1120] = every tetromino piece (7) * every rotation of a piece (4) * every position (10) * evaluations (4)
// observations[1120 to 1320] = State of the board
// evaluations:
//  - # of lines that will be cleared
//  - # of holes in the grid
//  - grid bump
//  - grid sum height
```
2. The previous solution has a very large observation space, and there is a very likely chance for the hidden layer to have dead neurons since every piece and roation wont be too valuable. Thus, we can try to lower the observation to the state of the board and possible evaluations.

```
// observations[0 to 1] = Current Tetromino Piece
// observations[1 to 201] = State of the board
// observations[201 to 201] = State of the board


```

- This is how we get a single observation:
```
public float[] GetSingleObservationMove(int x, int rot) {
        this.game.ClearCurrentTetromino();
        // save current position
        Vector3Int savePos = this.pos;
        Vector3Int[] saveTetrominoPositions = this.tetrominoPositions;

        Vector2Int newPos = new Vector2Int(x, this.pos.y);

        // move
        SetTetromino(newPos);

        while(MoveTetromino(Vector2Int.down)){
            continue;
        }
        // rotate
        for(int i = 0; i < rot; i++)
            RotateRight();

        // view change        
        this.game.SetCurrentTetromino();

        // GET OBSERVATIONS
        float[] result = GetSingleObservation(this.game.TileMap, x, rot);

        // reset to previous
        this.game.ClearCurrentTetromino();
        this.pos = savePos;
        this.tetrominoPositions = saveTetrominoPositions;
        this.game.SetCurrentTetromino();

        return result;
    }
```
A small bug here is the movement before the rotation. There are some errors with rotation before and rotation after. For example:

if I can not rotate at the start, there are many places where we cannot find the proper place to drop the piece (the corner would want the piece to rotate first and then drop)

```
# # # x x x x x x x x
x # x x x x x x x x x 
x x x x x x x x x x x
x x x x x x x x x x x 
x x x x x x x x x x x
x x x x x x x x x x x 
x x x x x x x x x x x
x x x x x x x # x x # 
x x x x x # # # # x #
x # # # # # # # # # # 
```


if I can rotate at the start, the t piece on the top will not be able to t-spin
```
# # # x x x x x x x x
x # x x x x x x x x x 
x x x x x x x x x x x
x x x x x x x x x x x 
x x x x x x x x x x x
x x x x x x x x x x x 
x x x x # x x x x x x
x x x x # # x # x x # 
# x x x x # # # # x #
# # # x # # # # # # # 
```

In order to solve this problem, we have two ways to approach this, either way the observation space will have to increase or we will rely on some intuition.

Furthermore, there is another problem obtaining the observations, it may seem incredibly hard for the AI to understand the usefulness of aggregate height, bumpiness, etc if the results of which are not forecasted. Therefore, those numbers produced by the observations are not relevant unless it leads to the reward increasing. Thus, we need to find a way for the agent to gain reward for finding the right heuristics:

i.e. in the doc describing the heuristics:
```
a * (aggregate height) + b * (complete lines) + c * (holes) + d * (bumpiness)

We want to minimize aggregate height, holes and bumpiness, so we can expect a, c, d to be negative. 
Similarly, we want to maximize the number of complete lines, so we can expect B to be positive.
```

Currently these numbers are not represented correctly as we do not attempt to minimize or maximize any of such. 



### Solutions

Let's pretend that we have this state:

```
# # # x x x x x x x x
x # x x x x x x x x x 
x x x x x x x x x x x
x x x x x x x x x x x 
x x x x x x x x x x x
x x x x x x x x x x x 
x x x x # x x x x x x
x x x x # # x # x x # 
# x x x x # # # # x #
# # # x # # # # # # # 
```

if we look at every possible observation utilizing the function to find what the piece will do, it will "in theory" look for every single way to 


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