﻿/****************************************************************************************
	
	Algorithm by r57shell & feos, 2018

	_zeros is the key to GREENZONE DECAY PATTERN.

	In a 16 element example, we evaluate these bitwise numbers to count zeros on the right.
	First element is always assumed to be 16, which has all 4 bits set to 0. Each right zero
	means that we lower the priority of a state that goes at that index. Priority changes
	depending on current frame and amount of states. States with biggest priority get erased
	first. With a 4-bit battern and no initial gap between states, total frame coverage is
	about 5 times state count. Initial state gap can screw up our patterns, so do all
	calculations like gap isn't there, and take it back into account afterwards.

	_zeros values are essentialy the values of rshiftby here:
	bitwise view     frame    rshiftby priority
	  00010000         0         4         1
	  00000001         1         0        15
	  00000010         2         1         7
	  00000011         3         0        13
	  00000100         4         2         3
	  00000101         5         0        11
	  00000110         6         1         5
	  00000111         7         0         9
	  00001000         8         3         1
	  00001001         9         0         7
	  00001010        10         1         3
	  00001011        11         0         5
	  00001100        12         2         1
	  00001101        13         0         3
	  00001110        14         1         1
	  00001111        15         0         1

*****************************************************************************************/
using System.Collections.Generic;

namespace BizHawk.Client.Common
{
	internal class StateManagerDecay
	{
		private TasStateManager _tsm;	// access tsm methods to make life easier
		private List<int> _zeros;		// amount of least significant zeros in bitwise view (also max pattern step)
		private int _bits;				// size of _zeros is 2 raised to the power of _bits
		private int _mask;				// for remainder calculation using bitwise instead of division
		private int _base;				// repeat count (like fceux's capacity). only used by aligned formula
		private int _capacity;			// total amount of savestates
		private int _step;				// initial memory state gap
		private bool _align;			// extra care about fine alignment. TODO: do we want it?

		public StateManagerDecay(TasStateManager tsm)
		{
			_tsm = tsm;
			_align = false;
		}

		// todo: go through all states once, remove as many as we need. refactor to not need goto
		public void Trigger(int decayStates)
		{
			for (; decayStates > 0 && _tsm.StateCount > 1;)
			{
				int baseStateIndex = _tsm.GetStateIndexByFrame(Global.Emulator.Frame);
				int baseStateFrame = _tsm.GetStateFrameByIndex(baseStateIndex) / _step;	
				int forwardPriority = -1000000;
				int backwardPriority = -1000000;
				int forwardFrame = -1;
				int backwardFrame = -1;

				for (int currentStateIndex = 1; currentStateIndex < baseStateIndex; currentStateIndex++)
				{
					int currentFrame = _tsm.GetStateFrameByIndex(currentStateIndex);

					if (_tsm.StateIsMarker(currentFrame))
					{
						continue;
					}
					else if (currentFrame % _step > 0)
					{
						// ignore the pattern if the state doesn't belong already, drop it blindly and skip everything
						_tsm.RemoveState(currentFrame);
						decayStates--;

						// this is the kind of highly complex loops that might justify goto
						goto next_state;
					}
					else
					{
						currentFrame /= _step;
					}

					int zeroCount = _zeros[currentFrame & _mask];
					int priority = ((baseStateFrame - currentFrame) >> zeroCount);

					if (_align)
					{
						priority -= ((_base * ((1 << zeroCount) * 2 - 1)) >> zeroCount);
					}

					if (priority > forwardPriority)
					{
						forwardPriority = priority;
						forwardFrame = currentFrame;
					}
				}

				for (int currentStateIndex = _tsm.StateCount - 1; currentStateIndex > baseStateIndex; currentStateIndex--)
				{
					int currentFrame = _tsm.GetStateFrameByIndex(currentStateIndex) / _step;

					if (_tsm.StateIsMarker(currentFrame))
					{
						continue;
					}
					else if (currentFrame % _step > 0)
					{
						// ignore the pattern if the state doesn't belong already, drop it blindly and skip everything
						_tsm.RemoveState(currentFrame);
						decayStates--;

						// this is the kind of highly complex loops that might justify goto
						goto next_state;
					}
					else
					{
						currentFrame /= _step;
					}

					int zeroCount = _zeros[currentFrame & _mask];
					int priority = ((currentFrame - baseStateFrame) >> zeroCount);

					if (_align)
					{
						priority -= ((_base * ((1 << zeroCount) * 2 - 1)) >> zeroCount);
					}

					if (priority > backwardPriority)
					{
						backwardPriority = priority;
						backwardFrame = currentFrame;
					}
				}

				if (forwardFrame > -1 && backwardFrame > -1)
				{
					if (baseStateFrame - forwardFrame > backwardFrame - baseStateFrame)
					{
						_tsm.RemoveState(forwardFrame * _step);
					}
					else
					{
						_tsm.RemoveState(backwardFrame * _step);
					}

					decayStates--;
				}
				else if (forwardFrame > -1)
				{
					_tsm.RemoveState(forwardFrame * _step);
					decayStates--;
				}
				else if (backwardFrame > -1)
				{
					_tsm.RemoveState(backwardFrame * _step);
					decayStates--;
				}
				else
				{
					// we're very sorry about failing to find states to remove, but we can't go beyond capacity, so remove at least something
					// this shouldn't happen, but if we don't do it here, nothing good will happen either
					_tsm.RemoveState(_tsm.GetStateFrameByIndex(1));
				}

				// this is the kind of highly complex loops that might justify goto
				next_state:;
			}
		}

		public void UpdateSettings(int capacity, int step, int bits)
		{
			_capacity = capacity;
			_step = step;
			_bits = bits;
			_mask = (1 << _bits) - 1;
			_base = (_capacity + _bits / 2) / (_bits + 1);
			_zeros = new List<int>();
			_zeros.Add(_bits);

			for (int i = 1; i < (1 << _bits); i++)
			{
				_zeros.Add(0);

				for (int j = i; j > 0; j >>= 1)
				{
					if ((j & 1) > 0)
					{
						break;
					}

					_zeros[i]++;
				}
			}
		}
	}
}
