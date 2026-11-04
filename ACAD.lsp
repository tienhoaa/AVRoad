;; ========================================================
;; AVRoad 2026 Solution - Startup Configuration
;; ========================================================

;; Initialize Express Tools Autoloading cleanly
(if (findfile "acetutil.arx")
  (arxload "acetutil.arx")
)
(if (findfile "acetauto.lsp")
  (load "acetauto.lsp")
)

;; Command wrappers for Properties palette (Ctrl+1)
(defun c:properties ()
  (vl-cmdf "_.properties")
  (princ)
)

(defun c:propertiesclose ()
  (vl-cmdf "_.propertiesclose")
  (princ)
)

(defun S::STARTUP()
  (setvar "CMDECHO" 0)
  (vl-catch-all-apply '(lambda ()
    ;; Set Command Line font to Segoe UI
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
  ))
  (setvar "CMDECHO" 1)
  (princ "\nAVRoad 2026 Solution - S\U+1EB5n s\U+00E0ng l\U+00E0m vi\U+1EC7c.\n")
  (princ)
)
