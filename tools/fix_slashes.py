with open('AVRoad_Launcher.cs', 'r', encoding='utf-8') as f:
    code = f.read()

# Replace double backslashes in verbatim strings in RegisterAutoCADApplications
code = code.replace('@\"Software\\\\Autodesk\\\\AutoCAD\\\\R17.0\\\\ACAD-5001:409\\\\Applications\"', '@\"Software\\Autodesk\\AutoCAD\\R17.0\\ACAD-5001:409\\Applications\"')
code = code.replace('@\"Software\\\\WOW6432Node\\\\Autodesk\\\\AutoCAD\\\\R17.0\\\\ACAD-5001:409\\\\Applications\"', '@\"Software\\WOW6432Node\\Autodesk\\AutoCAD\\R17.0\\ACAD-5001:409\\Applications\"')
code = code.replace('@\"Software\\\\Autodesk\\\\AutoCAD\\\\R17.0\\\\ACAD-5001:409\\\\Profiles\\\\AVRoad\\\\Dialogs\\\\LayerManager\"', '@\"Software\\Autodesk\\AutoCAD\\R17.0\\ACAD-5001:409\\Profiles\\AVRoad\\Dialogs\\LayerManager\"')
code = code.replace('@\"Software\\\\Autodesk\\\\AutoCAD\\\\R17.0\\\\ACAD-5001:409\\\\Profiles\\\\AVRoad\\\\Command Line Windows\"', '@\"Software\\Autodesk\\AutoCAD\\R17.0\\ACAD-5001:409\\Profiles\\AVRoad\\Command Line Windows\"')

with open('AVRoad_Launcher.cs', 'w', encoding='utf-8') as f:
    f.write(code)

with open('Setup_AVRoad_2026.cs', 'r', encoding='utf-8') as f:
    code2 = f.read()

code2 = code2.replace('@\"Software\\\\Autodesk\\\\AutoCAD\\\\R17.0\\\\ACAD-5001:409\\\\Applications\"', '@\"Software\\Autodesk\\AutoCAD\\R17.0\\ACAD-5001:409\\Applications\"')
code2 = code2.replace('@\"Software\\\\WOW6432Node\\\\Autodesk\\\\AutoCAD\\\\R17.0\\\\ACAD-5001:409\\\\Applications\"', '@\"Software\\WOW6432Node\\Autodesk\\AutoCAD\\R17.0\\ACAD-5001:409\\Applications\"')
code2 = code2.replace('@\"Software\\\\Autodesk\\\\AutoCAD\\\\R17.0\\\\ACAD-5001:409\\\\Profiles\\\\AVRoad\\\\Dialogs\\\\LayerManager\"', '@\"Software\\Autodesk\\AutoCAD\\R17.0\\ACAD-5001:409\\Profiles\\AVRoad\\Dialogs\\LayerManager\"')

with open('Setup_AVRoad_2026.cs', 'w', encoding='utf-8') as f:
    f.write(code2)

print('Fixed verbatim string backslashes in both files!')
