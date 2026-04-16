import {Children, useEffect} from 'react';
import ReactDOM from 'react-dom';

function MedicalAidOptionViewModal({isOpen,onClose, title, data = []}) {
    // Getting the Columns
    const columns = data.length > 0 ? Object.keys(data[0]) : [];

    // Close on Escape Key
    useEffect(() => {
        if(!isOpen) return;

        const handleEscKeyDown = (evt) => evt.Key === "Escape" && onClose();
        document.addEventListener("keydown", handleEscKeyDown);
        return () => document.removeEventListener("keydown", handleEscKeyDown);
    },[isOpen,onClose]);

    // Prevent background scroll when open | Scroll Lock
    useEffect(() => {
        document.body.style.overflowY = isOpen ? "hidden" : "";
        return () => {document.body.style.overflow = ""};
    }, [isOpen]);

    // Disable background interaction when open
    useEffect(() => {
        const appRoot = document.getElementById("root");

        if (!appRoot) return ;

        if(isOpen) {
          appRoot.setAttribute("inert", "");
        } else {
            appRoot.removeAttribute("inert");
        }
         return () => appRoot.removeAttribute("inert");

    },[isOpen]);

    if (!isOpen) return null;

    // Render outside #root so inert doesn't block the modal
    return ReactDOM.createPortal(
        <div
          className="menu-background"
          onClick={onClose}
        >
          <div
            className="medicalAidOptions-modalContent model-wide"
            onClick={(e) => e.stopPropagation()}
          >

            {/* Header */}
            <div
              className="medicalAidOptions-header">
              <h2 id="modal-title">{title}</h2>
              <button
                className="medicalAidOptions-modalClose"
                onClick={onClose}
                aria-label="Close">
                  &times;
              </button>
            </div>

            {/* Body */}
            <div
              className="medicalAidOptions-modalBody"
            >
              {data.length === 0 ? (
                <p
                  className="medicalAidOptions-modalEmpty"
                >
                  No data available.
                </p>
                ) : (
                  <table
                    className="medicalAidOptions-modalViewTable">
                    <thead>
                      <tr>
                        {columns.map((col) => (
                          <th key={col}>
                              {/* Convert camelCase/snake_case keys into readable labels */}
                              {formatHeader(col)}
                          </th>
                        ))}
                      </tr>
                    </thead>
                    <tbody>
                    {data.map((row, rowIndex) => (
                      <tr key={row.id ?? rowIndex}>
                          {columns.map((col) => (
                            <td key={col}>
                                {/* Handles nested objects/arrays */}
                                {formatCell(row[col])}
                            </td>
                          ))}
                      </tr>
                    ))}
                    </tbody>

                  </table>
                )}
            </div>

            {/* Footer */}
            <hr className="medicalAidOptions-horizontal-line" />

            <div
              className="medicalAidOptions-modalFooter">
              <button
                className="btn btn-secondary"
                onClick={onClose}
              >
                Close
              </button>
            </div>

          </div>
        </div>,
        document.body
    );
}

// Helper Functions
// A Function to format column headers into readable labels
function formatHeader(key) {
  return key
    .replace(/_/g, " ")                       // Replaces underscores with spaces (for snake_case)
    .replace(/([A-Z]])/g, " $1")              // Adds space before capital letters (for camelCase)
    .replace(/^\w/, (c) => c.toUpperCase())   // Capitalize first Letter
    .trim();                                  // Trim out trailing spaces and whitspaces
}

// A function to safely render any cell value
function formatCell(value) {
  if (value === null || value === undefined) return "-";
  if (typeof(value) === "boolean") return value ? "Yes" : "No";
  if (typeof(value) === "object")  return JSON.stringify(value);

  return value;
}

export default MedicalAidOptionViewModal;