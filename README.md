```text
   __  __          __          __  ____               ___       _________   _____
  / / / /___  ____/ /__  _____/  |/  (_)___  ___     |__ \     /_  __/   | / ___/
 / / / / __ \/ __  / _ \/ ___/ /|_/ / / __ \/ _ \    __/ /     / / / /| | \__ \ 
/ /_/ / / / / /_/ /  __/ /  / /  / / / / / /  __/   / __/     / / / ___ |___/ /
\____/_/ /_/\__,_/\___/_/  /_/  /_/_/_/ /_/\___/   /____/    /_/ /_/  |_/____/ 
                   
```

# Usage

Currently only available on windows.

1. [Subscribe](https://steamcommunity.com/sharedfiles/filedetails/?id=3634683501) to the TAS mod in the workshop
2. Locate the TAS download directory, usually located in: `x:\Program Files (x86)\Steam\steamapps\workshop\content\1869780\3634683501`.
3. Remove the `Mod_0DeleteMe.txt` file from the TAS download directory.
4. Move the `Lithium.Core.Thor.Core.dll` to the UnderMine2 Managed directory, usually located in: `x:\Program Files (x86)\Steam\steamapps\common\Undermine2\UnderMine2_Data\Managed`.
5. Launch UnderMine2 and press f1 to open the TAS menu.

# Contributing
Feel free to fork the repository and submit pull requests for any improvements or bug fixes. Your contributions are welcome!

### Pre-requisites
- Unity 6000.0.36f1
- dotnet
- xcopy

### Notes

- Make sure to add the environment variables:
    - `UNITY_EDITOR_PATH` pointing to your Unity Editor installation (usually in `x:/Program Files/Unity 6000.0.36f1/Editor`).
    - `STEAMAPPS_PATH` pointing to your Steam `steamapps` directory (usually in `x:/Program Files (x86)/Steam/steamapps`).
