# Scp650plugin
Plugin for SCP-650 schematic of MER plugin based on Exiled framework of SCP: Secret Laboratory.

Requirements:
0. Exiled framework. https://github.com/Exiled-Team/EXILED
1. MapEditorReborn plugin. https://github.com/Michal78900/MapEditorReborn

Installation:
1. Put scp650 schematic folder to your %appdata%\Roaming\Exiled\Configs\MapEditorReborn\Schematics.
2. Install scp650plugin.dll to %appdata%\Roaming\Exiled\Plugins.
3. **Run Local Admin. And Exit.**
4. **Put global.yml to %appdata%\Roaming\Exiled\Configs\SCP-650 poses. global.yml is pose collection file.**
5. Check out whether it operates normally.

**Edit Poses/Create Poses:**
1. Download SL-CustomObjects. https://github.com/Michal78900/MapEditorReborn/releases/tag/2.1.2
2. Load scp650 schematic folder. By using 'import schematic' of bar 'schematic'.
3. Add 'Pose Recorder'script to root object: SCP-650.
4. Edit property 'size' of 'Objects' of the script to 13.
5. Drag all mixamorig:{joint name} to each Elements of the property.
5-1. If you're going to add more joint to edit, you can add more mixamorig.
6. **A primitive object with mixamorig in its name means that it will position a joint with the same name on same position.**
7. Make your poses. **SCALE DOESN'T SUPPORTED.**
8. Press play button. Then there will be a log on console tab.
9. Press it to see all of log content. And copy it.
10. Open global.yml. And write following.

```
- pose_name: {pose_name_here}
  transform_per_joint:
{paste_console_content_here}
```
11. If you want to set the pos to default:standing, just select all mixamorig and set rotation to 0 0 0.
