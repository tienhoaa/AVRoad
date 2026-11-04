;; ========================================================
;; AVRoad 2026 Solution - Startup Configuration
;; ========================================================

(vl-load-com)

;; Pre-load Essential AutoCAD Standard ARX Modules
(foreach arx '("units.arx" "AcDim.arx" "dwgaids.arx" "AcApp.arx" "AcArray.arx" 
               "acblock.arx" "textfind.arx" "AcPurge.arx" "acmatch.arx" 
               "appload.arx" "AcLayerP.arx" "AcTable.arx" "AcTp.arx" "laytrans.arx")
  (if (and (findfile arx) (not (member (strcase arx) (mapcar 'strcase (arx)))))
    (vl-catch-all-apply 'arxload (list arx nil))
  )
)

;; Initialize Express Tools Autoloading
(if (findfile "acetutil.arx")
  (vl-catch-all-apply 'arxload (list "acetutil.arx" nil))
)
(if (findfile "acetauto.lsp")
  (vl-catch-all-apply 'load (list "acetauto.lsp" nil))
)

;; Helper to ensure AcLayer is loaded when layer command is called
(defun c:layer ()
  (if (and (findfile "AcLayer.dll") (not (member "ACLAYER" (mapcar 'strcase (arx)))))
    (vl-catch-all-apply 'vl-cmdf (list "_.netload" (findfile "AcLayer.dll")))
  )
  (vl-cmdf "_.layer")
  (princ)
)

(defun S::STARTUP()
  (setvar "CMDECHO" 0)
  (vl-catch-all-apply '(lambda ()
    ;; Set Command Line font to Segoe UI / Arial
    (setenv "CmdLine.FontFace" "Segoe UI")
    (setenv "TextWindow.FontFace" "Segoe UI")
    (setenv "CmdLine.FontHeight" "10")
    (setenv "TextWindow.FontHeight" "10")

    ;; Smooth drafting and display optimization
    (setvar "WHIPARC" 1)
    (setvar "VIEWRES" 20000)
    (setvar "VTENABLE" 3)
    (setvar "VTDURATION" 150)
    (setvar "DRAGMODE" 2)
    (setvar "ZOOMFACTOR" 60)
    (setvar "CMDDIA" 1)
    (setvar "FILEDIA" 1)
    (setvar "ATTDIA" 1)

    ;; Load AcLayer and AcLayerTools for Layer Properties Manager
    (if (findfile "AcLayer.dll")
      (vl-catch-all-apply 'vl-cmdf (list "_.netload" (findfile "AcLayer.dll")))
    )
    (if (findfile "AcLayerTools.dll")
      (vl-catch-all-apply 'vl-cmdf (list "_.netload" (findfile "AcLayerTools.dll")))
    )
  ))
  (setvar "CMDECHO" 1)
  (princ "\nAVRoad 2026 Solution - S\U+1EB5n s\U+00E0ng l\U+00E0m vi\U+1EC7c.\n")
  (princ)
)
