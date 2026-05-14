# Luban DataTables

This folder contains the project Luban table definitions and source data.

- `luban.conf`: Luban export targets and schema/data roots.
- `Defines/`: XML schema definitions for beans, enums, and tables.
- `Datas/`: designer-editable table data.
- `gen_client.bat`: generates Unity C# code and JSON data.

Unity package dependency uses a fixed tag:

- `com.code-philosophy.luban`: `https://github.com/focus-creative-games/luban_unity.git#1.2.0`

Before running generation, copy Luban binaries to `Tools/Luban/Luban.dll`.
The script writes generated C# to `Assets/Scripts/Gen/Luban` and generated JSON to `Assets/Resources/LubanData`.

Do not point Luban output directories at existing hand-written code or asset folders; Luban clears output directories during generation.
