import {useEffect, useState} from 'react';
import ReactDOM from 'react-dom';
import './MedicalAidOptionViewModal.css';
import formatToLocalCurrency from "../../../../../utils/formatToLocalCurrency";
import formatSalaryBracket from "../../../../../utils/formatSalaryBracket";
import Divider from './Divider';


function MedicalAidOptionViewModal({isOpen, onClose, title, data = [], categories = [], categoryArray = []}) {
    // TODO : Prepare the data/transform to use within the model, with all it's category's relatives
    /*
    * Transform the flat data into a grouped structure by category
    * Pass all options in the selected category to the modal (not just one row)
    * Add a dropdown in the modal to select which category to view
    * Display grouped data showing option names and salary brackets per category
    * */

    //==== Work Area for proposed solution
    // Step 3 : Update MedicalAidOptionViewModal to accept and use categories
    const [selectedOptionId,setSelectedOptionId] = useState(null);
    const [categoryNameList, setCategoryNameList] = useState(null);
    const [categoryIncomeBrackets, setCategoryIncomeBrackets] = useState(null);
    const [selectedIncomeBracket, setSelectedIncomeBracket] = useState(null);
    const [isEditing, setIsEditing] = useState(false);

    const onEditClick = () => {
        (!isEditing) ?
            setIsEditing(true) :
            setIsEditing(false);
    }

    useEffect(() => {
      setCategoryNameList(categoryArray);
    },[categoryArray]);

    // Step1 : Transform the flat data into a grouped structure by category || Data transformer helper function
    const groupedOptionsByCategory = (options, categories) => {
      const grouped = {};

      options.forEach(option => {
        const categoryId = option.medicalOptionCategoryId;
        const category = categories.find(cat => cat.medicalOptionCategoryId === categoryId);
        const categoryName = category.medicalOptionCategoryName;
        const minimumSalary = option.salaryBracketMin;
        const maximumSalary = option.salaryBracketMax;
        const optionSalaryBracket = formatSalaryBracket(minimumSalary, maximumSalary, formatToLocalCurrency);




          if(!grouped[categoryId]){
          grouped[categoryId] = {
            categoryId,
            categoryName,
            incomeBrackets: [],
            options: []
          };
        }

        grouped[categoryId].incomeBrackets.push(optionSalaryBracket);
        grouped[categoryId].options.push(option);
      });
      return Object.values(grouped);
    };

    // Group data by category (Part of Part 3)
    const groupedData = groupedOptionsByCategory(data, categories);

    // Flatten all options with category info for dropdown
    const flattenedOptions = groupedData.flatMap(category =>
    category.options.map(option => ({
        ...option,
        categoryName: category.categoryName ? category.categoryName : category.categoryId
    }))
    );

    // Set default category on data load
    useEffect(() => {
      if(groupedData.length > 0 && !selectedOptionId){
        setSelectedOptionId(flattenedOptions[0].medicalOptionId);
      }
    },[flattenedOptions, selectedOptionId]);

// Get the currently selected option
    const displayData = selectedOptionId ? flattenedOptions.filter(option => option.medicalOptionId === selectedOptionId) : [];
// Get the currently selected category's Income Brackets
    useEffect(()=>{
      // If the grouped data is not null, then proceed with extracting the income brackets for that category group
      if(groupedData.length > 0){
        const allIncomeBrackets = groupedData.map(category => category.incomeBrackets);
        const flattenedBrackets = allIncomeBrackets.flat();
        setCategoryIncomeBrackets(flattenedBrackets);

        const allIncomeBrackets2 = groupedData.reduce((acc, category) => {
          acc[category.categoryId] = category.incomeBrackets;
          return acc;
        },{});
        console.log(":=------------------ Start of Income Bracket Dump ------------------=:");
        console.log(allIncomeBrackets2);
        console.log(":=------------------ End of Dump ------------------=:");
      }
    },[groupedData])


    // Step 4 : Add the dropdown in the modal body (before the table)

    //==== End Work Area for proposed solution





    const headerColumnNames = [
      {
        header: "Component", key:"component", render: () => {

        }
      },
      {
        header: "Principal" , key: "principal", render: () => {

        }
      },
      {
        header: "Adult" , key: "adult", render: () => {

        }
      },
      {
        header: "Child" , key: "child", render: () => {

        }
      }
    ];

    const componentTypeNames = [
      {
        header: "Monthly Risk Contribution", key: "monthlyRiskContribution", render: () => {

        }
      },
      {
        header: "Risk Allocation", key: "riskAllocation", render: () => {

        }
      },
      {
        header: "Monthly Saving Account (MSA)", key: "monthlySavingAccount", render: () => {

        }
      },
      {
        header: "MSA Allocation", key: "msaAllocation", render: () => {

        }
      },
    ];

    const totalDeductionsHeaderNames = [
      {
        header: "Principal", key:"principal", render: () => {

        }
      },
        {
            header: "Adult", key: "adult", render: () => {

            }
        },
        {
            header: "1st Child", key: "firstChild", render: () => {

            }
        },
        {
            header: "2nd Child +", key: "children", render: () => {

            }
        },
    ];


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

                  const newFormat = formatSalaryBracket(min,max, formatToLocalCurrency);

                  return newFormat;
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
                  {flattenedOptions.length > 0 && (
                    <div className="category-selector">
                      <label htmlFor="category-dropdown">MEDICAL OPTION</label>
                      <select
                        id="category-dropdown"
                        value={selectedOptionId || ''}
                        onChange={(e) => setSelectedOptionId(Number(e.target.value))}
                        className="category-dropdown"
                      >
                        {flattenedOptions.map(option => (
                          <option
                            key={option.categoryId}
                            value={option.medicalOptionId}> {/*can change the category from int to string here*/}
                              {option.medicalOptionName}
                          </option>
                        ))}
                      </select>
                    </div>


                  )}

                  {categoryIncomeBrackets && categoryIncomeBrackets.length > 0 && (


                      (!isEditing)  ? (
                        <div className="income-bracket-selector">
                          <label htmlFor="income-bracket-dropdown">INCOME CATEGORY</label>
                          <p>NOTHING TO DISPLAY PAL :)</p>
                        </div>
                      ) : (
                        <div className="income-bracket-selector">
                          <label htmlFor="income-bracket-dropdown">INCOME CATEGORY</label>

                          <select
                            id="income-bracket-dropdown"
                            value={selectedIncomeBracket || ''}
                            onChange={(e) => setSelectedIncomeBracket(e.target.value)}
                            className="income-bracket-dropdown"
                          >
                            <option value="">
                            </option>
                            {[...new Set(categoryIncomeBrackets)].map((bracket, index) => (
                              <option
                                key={index}
                                value={bracket}
                              >
                                {bracket}
                              </option>
                            ))}
                          </select>
                        </div>
                      )

                  )}
                  <div className="section-divider">
                    <p>
                      MONTHLY CONTRIBUTION BREAKDOWN
                    </p>
                    <Divider />
                  </div>

                  <table
                      className="modal-view-table">
                      <thead className="modal-view-table-header">
                      <tr>

                          {headerColumnNames.map((col, colIndex) => (
                              <th
                                className="model-view-table-header-cell">
                                <tr key={col.key ?? colIndex}>


                                </tr>
                              {/* Convert camelCase/snake_case keys into readable labels */}
                                  {formatHeader(col.header)}
                          </th>
                        ))}
                      </tr>
                    </thead>
                    <tbody>
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
                      <div className="section-divider">
                          <p>
                              TOTAL MONTHLY DEDUCTION
                          </p>
                          <Divider />
                      </div>
                      {/* Sections that hold the total monthly deduction (dynmically calculated) */}

                      <Divider />
                      {/* Footer */}
            </>
                )}
            </div>






            <div
              className="medicalAidOptions-modalFooter">
              <button
                className="btn btn-secondary"
                onClick={onClose}
              >
                Cancel
              </button>
              <button
                  className="btn btn-secondary"
                  onClick={onEditClick}
              >
               Edit Plan
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