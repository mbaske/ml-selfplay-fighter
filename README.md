## Self-Play Fighter - [Video](https://youtu.be/tIy3HmXz05A)

Two humanoid agents compete in a boxing match. This is an update of an older project, made with [Unity Machine Learning Agents](https://github.com/Unity-Technologies/ml-agents). You can find the previous version here [https://github.com/mbaske/ml-selfplay-fighter/tree/Version1](https://github.com/mbaske/ml-selfplay-fighter/tree/Version1)

The included policy was trained with PPO in two stages: 
1) Agents first learn to balance themselves, being rewarded for keeping their body root upright.
2) Using adversarial self-play, agents are then trained to hit upper body parts of their opponents. An agent wins if its cumulative hit strength exceeds a given threshold. This threshold is updated during training, adapting to agents' skill levels. 

An issue with the previous version was that agents were too busy balancing themselves by sticking their arms and legs out, preventing them from fighting more effectively. I've tried to counteract this by adding maximum hand-to-hand and foot-to-foot distances an agent must abide by. Moving beyond these limits is factored into the rewards an agent receives during its initial training phase. As a result, agents now keep their feet together and balance themselves by moving their hands around in front of their body. This is somewhat less stable than before and agents are more prone to falling over. On the other hand, it's a good starting point for adversarial self-play training. Besides an agent exceeding the cumulative hit strength threshold, episodes can end under the following conditions:

* An agent exceeds the maximum hand or foot distances for longer than a given number of update steps. In that case, the agent loses and its opponent wins.
* Agents don't touch each other for a given number of update steps, which results in a draw.
* An agent falls over. This is counted as a draw too, in order to prevent early knock-out punches. Although that would be a good winning strategy, it doesn't make for very interesting episodes. Agents would try to end rounds quickly by hitting as hard as they can, or by smashing into opponents to bring them down.

Agents receive a small additional reward during self-play for facing each other and for proximity.

I've made a few minor tweaks during training, but didn't want to retrain from scratch every time. Hands have a minimum strength property, controlling whether hits are counted. I started out with a low value and increased it as agents got better. (This might be worth automating together with the cumulative strength threshold at some point.) I've also added a bit of stabilizing counter torque, helping agents with balancing themselves. They can still fall over, just not as easily.

</br></br>

Boxing Ring by Bamgbalat  
[https://sketchfab.com/3d-models/boxing-ring-117f158d570e45999d343f0c4478cc22](https://sketchfab.com/3d-models/boxing-ring-117f158d570e45999d343f0c4478cc22)    
