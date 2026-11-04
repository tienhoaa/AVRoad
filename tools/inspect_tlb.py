import ctypes
from ctypes import wintypes
import os

oleaut32 = ctypes.windll.oleaut32

# LoadTypeLib
ptlib = ctypes.c_void_p()
path = r"C:\AVRoad_2026\acax17enu.tlb"
hr = oleaut32.LoadTypeLib(path, ctypes.byref(ptlib))
print("LoadTypeLib hr:", hr)

# Let's inspect type infos using ctypes COM calling
# ITypeLib vtable:
# 0: QueryInterface, 1: AddRef, 2: Release
# 3: GetTypeInfoCount, 4: GetTypeInfo, 5: GetTypeInfoType, 6: GetTypeInfoOfGuid
# 7: GetLibAttr, 8: GetTypeComp, 9: GetDocumentation, 10: IsName, 11: FindName, 12: ReleaseTLibAttr

class TLIBATTR(ctypes.Structure):
    _fields_ = [
        ('guid', ctypes.c_byte * 16),
        ('lcid', wintypes.DWORD),
        ('syskind', wintypes.DWORD),
        ('wMajorVerNum', wintypes.WORD),
        ('wMinorVerNum', wintypes.WORD),
        ('wLibFlags', wintypes.WORD)
    ]

# Call GetTypeInfoCount (index 3)
vtable = ctypes.cast(ptlib, ctypes.POINTER(ctypes.POINTER(ctypes.c_void_p))).contents
GetTypeInfoCount = ctypes.WINFUNCTYPE(ctypes.c_uint, ctypes.c_void_p)(vtable[3])
count = GetTypeInfoCount(ptlib)
print("TypeInfo Count in acax17enu.tlb:", count)

# Call GetDocumentation (index 9)
# HRESULT GetDocumentation(INT index, BSTR *pBstrName, BSTR *pBstrDocString, DWORD *pdwHelpContext, BSTR *pBstrHelpFile)
GetDocumentation = ctypes.WINFUNCTYPE(
    wintypes.HRESULT, ctypes.c_void_p, ctypes.c_int,
    ctypes.POINTER(wintypes.BSTR), ctypes.POINTER(wintypes.BSTR),
    ctypes.POINTER(wintypes.DWORD), ctypes.POINTER(wintypes.BSTR)
)(vtable[9])

bstrName = wintypes.BSTR()
hr = GetDocumentation(ptlib, -1, ctypes.byref(bstrName), None, None, None)
print("Lib Name:", bstrName._bstr() if hasattr(bstrName, '_bstr') else ctypes.wstring_at(bstrName))

coclasses = []
GetTypeInfo = ctypes.WINFUNCTYPE(wintypes.HRESULT, ctypes.c_void_p, ctypes.c_uint, ctypes.POINTER(ctypes.c_void_p))(vtable[4])
GetTypeInfoType = ctypes.WINFUNCTYPE(wintypes.HRESULT, ctypes.c_void_p, ctypes.c_uint, ctypes.POINTER(ctypes.c_int))(vtable[5])

# TYPEKIND enum: TKIND_COCLASS = 5
for i in range(count):
    tkind = ctypes.c_int()
    GetTypeInfoType(ptlib, i, ctypes.byref(tkind))
    name = wintypes.BSTR()
    GetDocumentation(ptlib, i, ctypes.byref(name), None, None, None)
    n_str = ctypes.wstring_at(name) if name else ""
    if tkind.value == 5: # COCLASS
        coclasses.append((i, n_str))

print(f"Total COCLASSES: {len(coclasses)}")
print("First 20 coclasses:", [c[1] for c in coclasses[:20]])
