// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#include <stdio.h>
#include <tchar.h>


#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS      // some CString constructors will be explicit

#include <atlbase.h>
#include <atlstr.h>

// TODO: reference additional headers your program requires here

#import "constant.tlb" /* SolidEdgeConstants */
#import "framewrk.tlb" exclude ("UINT_PTR", "LONG_PTR") rename ("GetOpenFileName", "SEGetOpenFileName") /* SolidEdgeFramework */
#import "geometry.tlb" /* SolidEdgeGeometry */
#import "fwksupp.tlb" /* SolidEdgeFrameworkSupport */
#import "part.tlb" /* SolidEdgePart */
#import "assembly.tlb" /* SolidEdgeAssembly */
#import "draft.tlb" /* SolidEdgeDraft */
#import "SEInstallData.dll" /* SEInstallDataLib */

#define IfFailGo(x) { hr=(x); if (FAILED(hr)) goto Error; }
#define IfFailGoCheck(x, p) { hr=(x); if (FAILED(hr)) goto Error; if(!p) {hr = E_FAIL; goto Error; } }
