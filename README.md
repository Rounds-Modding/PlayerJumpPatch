# PlayerJump Patch
------------------

This is a utility mod which patches the erroneous `PlayerJump.Jump` method in the base game code.

Previously, changing `CharacterData.jump` to anything greater than `1` would give the player infinite jumps. This patch fixes that. This allows custom cards to change the number of jumps a player has to any arbitrary (positive) value, without having to patch the code themselves.

## Changelog
------------

- v0.0.2: Added fix for jump particles spawning in the wrong location
