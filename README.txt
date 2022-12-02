Insturctions to create a virtual environment:

Install Unity (2021.3 or later)
Install Python (3.8.13 or higher)

1. cd to the game directory

setup directory venv as new virtual environment
2. py -m venv venv 

get new pip
3. python -m pip install --upgrade pip

install pytorch
4. pip3 install torch~=1.7.1 -f https://download.pytorch.org/whl/torch_stable.html

install mlagents
5. python -m pip install mlagents==0.30.0

To view a description of all the CLI options accepted by mlagents-learn, use the --help:
mlagents-learn --help

The basic command for training is:
mlagents-learn <trainer-config-file> --env=<env_name> --run-id=<run-identifier>
mlagents-learn config/agent-config.yaml --run-id BehaviorPPO
