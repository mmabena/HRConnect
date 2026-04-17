import {useEffect} from 'react';
import ReactDOM from 'react-dom';
import './MedicalAidOptionViewModal.css';

function MedicalAidOptionViewModal({isOpen,onClose, title, data = []}) {
    // Getting the Columns || no need need to explicitly define them so that i can render them with custom logic
    const viewColumns = [
        {
            header: "Option Name", key: "medicalOptionName", width: 1, render: () => {

            }
        },
        {
            header: "Category", key: "medicalOptionCategoryId", width: 1, render: () => {

            }
        },
        {
            header: "Min Salary", key: "salarybracketMin", width: 1, render: () => {

            }
        },
        {
            header: "Maximum Salary", key: "salarybracketMax", width: 1, render: () => {

            }
        },
        {
            header: "MSA principal", key: "monthlyMSAContributionPrincipal", width: 1, render: () => {

            }
        },
        {
            header: "MSA adult", key: "monthlyMSAContributionAdult", width: 1, render: () => {

            }
        },
        {
            header: "MSA Child", key: "monthlyMSAContributionChild", width: 1, render: () => {

            }
        },
        {
            header: "Risk Principal", key: "monthlyRiskContributionPrincipal", width: 1, render: () => {

            }
        },
        {
            header: "Risk Adult", key: "monthlyRiskContributionAdult", width: 1, render: () => {

            }
        },
        {
            header: "Risk Child", key: "monthlyRiskContributionChild", width: 1, render: () => {

            }
        },
        {
            header: "Risk Child2", key: "monthlyRiskContributionChild2", width: 1, render: () => {

            }
        },
        {
            header: "Total Principal", key: "totalMonthlyContributionsPrincipal", width: 1, render: () => {

            }
        },
        {
            header: "Total Adult", key: "totalMonthlyContributionsAdult", width: 1, render: () => {

            }
        },
        {
            header: "Total Child", key: "totalMonthlyContributionsChild", width: 1, render: () => {

            }
        },
        {
            header: "Total Child2", key: "totalMonthlyContributionsChild2", width: 1, render: () => {

            }
        }
    ];


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
            className="medicalaid-menu-background"
          onClick={onClose}
        >
          <div
              className="model-wrapper"
            onClick={(e) => e.stopPropagation()}
          >

            {/* Header */}
            <div
                className="modal-header-container">
                <div className="modal-header-main-text">{title}</div>
              <button
                  className="modal-button-close"
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
                      className="modal-view-table">
                      <thead className="modal-view-table-header">
                      <tr>
                          {viewColumns.map((col) => (
                              <th
                                  key={col}
                                  className="model-view-table-header-cell">
                              {/* Convert camelCase/snake_case keys into readable labels */}
                                  {formatHeader(col.header)}
                          </th>
                        ))}
                      </tr>
                    </thead>
                    <tbody>
                    {data.map((row, rowIndex) => (
                      <tr key={row.id ?? rowIndex}>
                          {viewColumns.map((col) => (
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
      .replace(/([A-Z]]) /g, " $1")              // Adds space before capital letters (for camelCase)
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