import {useEffect, useState} from 'react';
import ReactDOM from 'react-dom';
import './MedicalAidOptionViewModal.css';
import useLocalCurrencyFormat from "../../../../../hooks/useLocalCurrencyFormat";

function MedicalAidOptionViewModal({isOpen, onClose, title, data = [], categories = []}) {
    // TODO : Prepare the data/transform to use within the model, with all it's category's relatives
    /*
    * Transform the flat data into a grouped structure by category
    * Pass all options in the selected category to the modal (not just one row)
    * Add a dropdown in the modal to select which category to view
    * Display grouped data showing option names and salary brackets per category
    * */

    //==== Work Area for proposed solution
    // Step 3 : Update MedicalAidOptionViewModal to accept and use categories
    const [selectedCategoryId,setSelectedCategoryId] = useState(null);


    // Step1 : Transform the flat data into a grouped structure by category || Data transformer helper function
    const groupedOptionsByCategory = (options, categories) => {
      const grouped = {};

      options.forEach(option => {
        const categoryId = option.medicalOptionCategoryId;
        const category = categories.find(cat => cat.medicalOptionCategoryId === categoryId);
        const categoryName = category?.medicalOptionCategoryName || 'Unknown';

        if(!grouped[categoryId]){
          grouped[categoryId] = {
            categoryId,
            categoryName,
            options: []
          };
        }

        grouped[categoryId].options.push(option);
      });
      return Object.values(grouped);
    };

    // Group data by category (Part of Part 3)
    const groupedData = groupedOptionsByCategory(data, categories);
    // Set default category on data load
    useEffect(() => {
      if(groupedData.length > 0 && !selectedCategoryId){
        setSelectedCategoryId(groupedData[0].categoryId);
      }
    },[groupedData, selectedCategoryId]);

    // Get the currently selected category's options
    const selectedCategory = groupedData.find(cat => cat.categoryId === selectedCategoryId);
    const displayData = selectedCategory?.options || [];


    // Step 4 : Add the dropdown in the modal body (before the table)

    //==== End Work Area for proposed solution








    const {
        toLocalCurrency
    } = useLocalCurrencyFormat();
    // Getting the Columns || no need need to explicitly define them so that i can render them with custom logic
    const viewColumns = [
        {
            header: "Medical Option", key: "medicalOptionName", width: 1, render: () => {

            }
        },
        {// Header Title
            header: "Medical Option Category", key: "medicalOptionCategoryId", width: 1, render: () => {

            }
        },
        {
            header: "Income Salary", key: "incomeCategory", width: 1,
              render: (value, row) => {
                  const min = row.salaryBracketMin;
                  const max = row.salaryBracketMax;

                  // if max is null/undefined, render as uncapped with "+"
                  if((max === null || max === undefined) && (min > 0 && (min !== undefined || true)) ) {
                      return `${toLocalCurrency(min, "en-ZA")} +`;
                  }
                  if((max === undefined || max === null) && (min === undefined || min === null || min === 0)) {
                      return 'N/A';
                  }
                  //otherwise show capped range
                  return `${toLocalCurrency(min, "en-ZA")} - ${toLocalCurrency(max, "en-ZA")}`;
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
                  <>

                  {/*
                  // Step 4:Add the dropdown in the modal body (before the table)
                  Category Selector Dropdown */}
                  {groupedData.length > 0 && (
                    <div className="category-selector">
                      <label htmlFor="category-dropdown">Select Category:</label>
                      <select
                        id="category-dropdown"
                        value={selectedCategoryId || ''}
                        onChange={(e) => setSelectedCategoryId(Number(e.target.value))}
                        className="category-dropdown"
                      >
                        {groupedData.map(category => (
                          <option
                            key={category.categoryId}
                            value={category.categoryId}> {/*can change the caegory from int to string here*/}
                              {category.categoryName} ({category.options.length} options)
                          </option>
                        ))}
                      </select>
                    </div>
                  )}

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
                    {/* Step 5: Display the data || update the table to render to use 'displayData' instead of 'data' */}
                    {displayData.map((row, rowIndex) => (
                      <tr key={row.id ?? rowIndex}>
                          {viewColumns.map((col) => (
                            <td key={col}>
                                {/* Handles nested objects/arrays */}
                                {col.render ? col.render(row[col.key], row) : formatCell(row[col.key])}
                            </td>
                          ))}
                      </tr>
                    ))}
                    </tbody>

                  </table>
            </>
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