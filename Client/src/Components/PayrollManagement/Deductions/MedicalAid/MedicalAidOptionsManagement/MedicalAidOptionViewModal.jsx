import {useEffect, useMemo, useState} from 'react';
import ReactDOM from 'react-dom';
import './MedicalAidOptionViewModal.css';
import formatToLocalCurrency from "../../../../../utils/formatToLocalCurrency";
import formatSalaryBracket from "../../../../../utils/formatSalaryBracket";
import medicalAidOptionDynamicCalculator from "../../../../../utils/medicalAidOptionDynamicCalculator";
import Divider from './Divider';
import DynamicGrid from './DynamicGrid';


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
    //const [editedPayload, setEditedPayload] = useMemo([]);
    const [riskAllocationsCollection, setRiskAllocationCollection] = useState([]);
    const [msaAllocationsCollection, setMsaAllocationCollection] = useState([]);

    const {
      calculatePrincipalTotal,
      calculateAdultTotal,
      calcculateChildTotal,
      calculateChild2Total
    } = medicalAidOptionDynamicCalculator(formatToLocalCurrency);


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
    //const groupedData = groupedOptionsByCategory(data, categories);

    // Flatten all options with category info for dropdown
    /*const flattenedOptions = groupedData.flatMap(category =>
    category.options.map(option => ({
        ...option,
        categoryName: category.categoryName ? category.categoryName : category.categoryId
    }))
    );*/

    // the above have been replaced with the following that useMemo() for performance efficiency:
    const groupedData = useMemo(()=>
        groupedOptionsByCategory(data, categories),[data, categories]);

    const flattenedOptions = useMemo(() =>
      groupedData.flatMap(category =>
        category.options.map(option => ({
          ...option,
          categoryName: category.categoryName ? category.categoryName : category.categoryId
        }))),[groupedData]);


    // Set default category on data load
    useEffect(() => {
      if(groupedData.length > 0 && !selectedOptionId){
        setSelectedOptionId(flattenedOptions[0].medicalOptionId);
      }
    },[flattenedOptions, selectedOptionId]);

    // Extract the currently selected option => to get the calc values to use

// Get the currently selected option
//    const displayData = selectedOptionId ? flattenedOptions.filter(option => option.medicalOptionId === selectedOptionId) : [];
// Above has been replaced with a logic that uses useMemo() for performance efficiency
const displayData = useMemo(() =>
    selectedOptionId ? flattenedOptions.filter(option => option.medicalOptionId == selectedOptionId) : []
    ,[flattenedOptions, selectedOptionId]);

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

    const totalDeductionsHeaderNames = [
      {
        header: "Principal", key:"principal", render: (value, row) => {

        }
      },
      {
        header: "Adult", key: "adult", render: (value, row) => {

        }
      },
      {
        header: "1st Child", key: "firstChild", render: (value, row) => {

        }
      },
      {
        header: "2nd Child +", key: "children", render: (value, row) => {

        }
      },
    ];


    // Getting the Columns || no need to explicitly define them so that i can render them with custom logic
    const viewColumns = [
      {
        header: "Medical Option", key: "medicalOptionName", width: 1, render: (value, row) => {

        }
      },
      {// Header Title
        header: "Medical Option Category", key: "medicalOptionCategoryId", width: 1, render: (value, row) => {

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
        header: "Maximum Salary", key: "salarybracketMax", width: 1, render: (value, row) => {

        }
      },
      {
        header: "MSA principal", key: "monthlyMSAContributionPrincipal", width: 1, render: (value, row) => {

        }
      },
      {
        header: "MSA adult", key: "monthlyMSAContributionAdult", width: 1, render: (value, row) => {

        }
      },
      {
        header: "MSA Child", key: "monthlyMSAContributionChild", width: 1, render: (value, row) => {

        }
      },
      {
        header: "Risk Principal", key: "monthlyRiskContributionPrincipal", width: 1, render: (value, row) => {

        }
      },
      {
        header: "Risk Adult", key: "monthlyRiskContributionAdult", width: 1, render: (value, row) => {

        }
      },
      {
        header: "Risk Child", key: "monthlyRiskContributionChild", width: 1, render: (value, row) => {

        }
      },
      {
        header: "Risk Child2", key: "monthlyRiskContributionChild2", width: 1, render: (value, row) => {

        }
      },
      {
        header: "Total Principal", key: "totalMonthlyContributionsPrincipal", width: 1, render: (value, row) => {

        }
      },
      {
        header: "Total Adult", key: "totalMonthlyContributionsAdult", width: 1, render: (value, row) => {

        }
      },
      {
        header: "Total Child", key: "totalMonthlyContributionsChild", width: 1, render: (value, row) => {

        }
      },
      {
        header: "Total Child2", key: "totalMonthlyContributionsChild2", width: 1, render: (value, row) => {

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

    if (!isOpen){ return null;}

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
                <div className="modal-header-main-text">{title}</div> <br/>
                <div className="modal-header-main-secondary-text ">{flattenedOptions[0].categoryName}</div>
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


                      (isEditing)  ? (
                        <div className="income-bracket-selector">
                          <label htmlFor="income-bracket-dropdown">INCOME CATEGORY</label>
                          <p> -=: You are in Edit Mode :=- </p>
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
                                <tr key={col.key ?? colIndex}
                                  value={col.header}>
                                    {/* Convert camelCase/snake_case keys into readable labels */}
                                    {formatHeader(col.header)}
                                </tr>
                          </th>
                        ))}
                      </tr>
                    </thead>
                    <tbody>
                    {/* Monthly Risk Contributions Row => Spans over 4 columns */}
                    <tr colspan="100%" className="model-view-special-table-row">

                        <td colSpan="100%" className="model-view-special-table-row-cell">
                        MONTHLY RISK CONTRIBUTION
                        </td>
                        <td colSpan="100%">

                        </td>
                        <td colSpan="100%">

                        </td>
                        <td colSpan="100%">

                        </td>
                    </tr>
                    {/* Monthly Risk Contributions Breakdown Per cell expect cell 0 (index position) */}
                    <tr>
                        {/* Row Title */}
                        <td>
                          Risk Allocation
                        </td>
                        {/* Dynamic Data rendered here */}
                        <td>
                            {
                                (Number(displayData[0]?.monthlyRiskContributionPrincipal) === 0 ||
                                displayData[0]?.monthlyRiskContributionPrincipal === null ||
                                displayData[0]?.monthlyRiskContributionPrincipal === undefined)
                                    ? formatToLocalCurrency(displayData[0]?.monthlyRiskContributionAdult, "en-ZA")
                                    : "-"
                            }
                        </td>
                        <td>
                            {
                                (Number(displayData[0]?.monthlyRiskContributionAdult) === 0 ||
                                displayData[0]?.monthlyRiskContributionAdult === null ||
                                displayData[0]?.monthlyRiskContributionAdult === undefined)
                                    ? "-"
                                    : formatToLocalCurrency(displayData[0]?.monthlyRiskContributionAdult, "en-ZA")
                            }

                        </td>
                        <td>
                            {
                                (Number(displayData[0]?.monthlyRiskContributionChild) === 0 ||
                                displayData[0]?.monthlyRiskContributionChild === null ||
                                displayData[0]?.monthlyRiskContributionChild === undefined)
                                    ? "-"
                                    : formatToLocalCurrency(displayData[0]?.monthlyRiskContributionChild, "en-ZA")
                            }
                        </td>
                    </tr>
                    {/* Monthly Savings Account (MSA) Breakdown Per cell expect cell 0 (index position) */}
                    <tr colspan="100%" className="model-view-special-table-row">

                        <td colSpan="100%" className="model-view-special-table-row-cell">
                            MONTHLY SAVINGS ACCOUNT (MSA)
                        </td>

                        <td colSpan="100%">

                        </td>
                        <td colSpan="100%">

                        </td>
                        <td colSpan="100%">

                        </td>
                        <td colSpan="100%">

                        </td>
                    </tr>

                    <tr>
                        {/* Row Title */}
                        <td>
                            MSA Allocation
                        </td>
                        {/* Dynamic Data rendered here */}
                        <td>

                                {
                                    Number(displayData[0]?.monthlyMsaContributionPrincipal) === 0 ||
                                    displayData[0]?.monthlyMsaContributionPrincipal === null ||
                                    displayData[0]?.monthlyMsaContributionPrincipal === undefined
                                        ? formatToLocalCurrency(displayData[0]?.monthlyMsaContributionAdult, "en-ZA")
                                        : "-"
                                }


                        </td>
                        <td>
                            {displayData[0]?.monthlyAdultContributionAdult
                                ? formatToLocalCurrency(displayData[0]?.monthlyAdultContributionAdult)
                                : "-"}
                        </td>
                        <td>
                            {displayData[0]?.monthlyMsaContributionChild
                                ? formatToLocalCurrency(displayData[0]?.monthlyMsaContributionChild, "en-ZA")
                                : "-"}
                        </td>
                    </tr>
                    </tbody>

                  </table>
                      <div className="section-divider">
                      <p>
                              TOTAL MONTHLY DEDUCTION
                          </p>
                          <Divider />

                      </div>
                      <div className="monthly-deductions-grid">
                          {/* Dynamic Grid */}
                          <div className="monthly-deductions-grid-cell">
                              {/* Use the header list to display the total monthly deduction header Names */}
                          </div>
                      </div>


                      {/* Sections that hold the total monthly deduction (dynamically calculated) */}
                      {/* Either use a dynamic grid or a table to display the total monthly deduction with a conditional render on child2+ on the dynamic grid */}
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

  /* Nested Tenary Operations:
  * condition1
  ? valueIfTrue1
  : condition2
      ? valueIfTrue2
      : valueIfFalse2*/
}

export default MedicalAidOptionViewModal;