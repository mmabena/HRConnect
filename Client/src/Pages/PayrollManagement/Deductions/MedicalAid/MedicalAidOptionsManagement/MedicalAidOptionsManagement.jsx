import {useEffect, useState} from 'react';
import {
    useMedicalAidOptionContext
} from "../../../../../api/Context/PayrollManagement/Deductions/MedicalAidOptions/MedicalAidOptionsContext";
import {toast} from "react-toastify";
import '../../../../../Components/MenuBar/MenuBar.css';
import MedicalAidOptionViewModal
    from "../../../../../Components/PayrollManagement/Deductions/MedicalAid/MedicalAidOptionsManagement/MedicalAidOptionViewModal";
import DynamicGrid
    from '../../../../../Components/PayrollManagement/Deductions/MedicalAid/MedicalAidOptionsManagement/DynamicGrid';
import useEmpPagination from "../../../../../hooks/useEmpPagination";
import useLocalCurrencyFormat from "../../../../../hooks/useLocalCurrencyFormat";
import './MedicalAidOptionsManagement.css';

const MedicalAidOptionsManagement = () => {
  const [medicalOptions, setMedicalOptions] = useState([]);
  const [medicalOptionsCategory, setMedicalOptionsCategory] = useState([]);
  const [activeTab, setActiveTab] = useState("Deductions");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalData, setModalData] = useState([]);
  //const [activePage, setActivePage] = useState(1);
  const [initialized, setInitialized] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

    // Handler to open modal
    const handleViewRecord = (rowData) => {
        setModalData([rowData] || []);  // Set data if you have row data
        setIsModalOpen(true);
    };

    // Handler to close modal
    const handleCloseModal = () => {
        setIsModalOpen(false);
    };

    // Use the employee pagination hook
    const {
        activePage,
        setActivePage,
        currentItems,
        totalPages,
        itemsPerPage
    } = useEmpPagination(medicalOptions, 10);

    const {
      toLocalCurrency
    } = useLocalCurrencyFormat();

    const {
        medicalAidOptions,
        medicalAidOptionsCategories,
        salaryBasedOptions,
        eligibleOptionsForEmployee,
        // Callback Functions
        getAllOptionsGroupedByCategory,
        getAllMedicalOptionsCategories,
        getMedicalOptionsSnapshot,
        getCategoryById,
        getMedicalOptionsByCategoryId,
        getMedicalOptionsSalaryBracketMatchingEmployeeSalary,
        getMemberEligibilityOptionsByEmployeeId,
        createMedicalOptionCategory,
        createBulkMedicalOptionCategoryOptionsByCategoryId,
        updateCategoryById,
        updateBulkMedicalOptionsByCategoryId,
    } = useMedicalAidOptionContext();

    const columns = [
      {header: "Option Name", key: "medicalOptionName", width: 1},
      {header: "Category", key: "medicalOptionCategoryName", width: 1,
        render: (value,row) => {
          const medicalCategoryId = row.medicalOptionCategoryId;

          // If no category ID or no categories loaded , return N/A
          if(!medicalCategoryId || medicalOptionsCategory.length === 0){
            return "N/A88";
          }

          // Lookup the category by matching the ID (Linq-like query)
          const category = medicalOptionsCategory.find(
              cat => cat.medicalOptionCategoryId === medicalCategoryId
          );

          //Return the category name if found, otherwise N/A
          return category ? category.medicalOptionCategoryName : "N/A777";
        }},
      // Custom render for salary bracket/category
      {header: "Income Category Salary", key: "salaryBracket", width: 1,
        render: (value, row) => {
          const min = toLocalCurrency(row.salaryBracketMin,"en-ZA");
            const max = row.salaryBracketMax;

          // if max is null/undefined, render as uncapped with "+"
          if(max === null || max === undefined) {
              return `${min} +`;
          }
          if((max === undefined || max === null)&&(min === undefined || min === null)) {
              return 'N/A';
          }
          //otherwise show capped range
            return `${min} - ${toLocalCurrency(max, "en-ZA")}`;
        }},
      {header: "Principal", key: "totalMonthlyContributionsPrincipal", width:1 ,
        render: (value) => toLocalCurrency(value,"en-ZA")},
      {header: "Adult", key: "totalMonthlyContributionsAdult", width:1 ,
        render: (value) => toLocalCurrency(value,"en-ZA")},
      {header: "1st Child", key: "totalMonthlyContributionsChild", width:1 ,
        render: (value) => toLocalCurrency(value,"en-ZA")},
      {header: "2nd Child +", key: "totalMonthlyContributionsChild2", width:1 ,
        render: (value) => toLocalCurrency(value,"en-ZA")},
      {header: "Actions", key: "actions", width:2,
        render: (value, row) => (
            <div className='edicalaid-options-actions-container'>
          <button
              className="medicalaid-options-borderless-button"
              onClick={() => handleViewRecord(row)}
          >
              View
          </button>
                <button
                    className="medicalaid-options-borderless-button"
                >
                    Edit
                </button>
            </div>
        )},
  ];

  //use effects
  useEffect(() => {
    const initializeOptions = async () => {
      try{
        console.log("-----------=: Initialization of Medical options And Categories on mount :=------------");
          const data = await getMedicalOptionsSnapshot();
          const categoryData = await getAllMedicalOptionsCategories();
        setInitialized(true);
        console.log("-----------=: Medical Aid Options And Categories Data loaded :=------------");
        console.log("-----------=: Dump Options :=-----------");
        console.log(data);
        console.log("-----------=: Dump Options :=-----------");
          console.log(categoryData);
        //set global data
        setMedicalOptions(data);
          setMedicalOptionsCategory(categoryData);
      }
      catch (error) {
        console.error(`-----------=: Error Caught :=------------\n\n${error}`);
        setError(error);
        toast.error('Failed to load Medical Aid options!');
      }
      finally{
        setLoading(false);
      }
    };

    initializeOptions();
  },[]);

  const pageTabs = [
      {
        label: "Earnings",
        value: "Earning"
      },
      {
        label: "Deductions",
        value: "Deductions"
      },
      {
        label: "Company Contributions",
        value: "Company Contributions"
      },
      {
        label: "BCEA",
        value: "BCEA"
      },
      {
        label: "OID",
        value: "OID"
      },
      {
        label: "Stock",
        value: "Stock"
      }
  ]

  // Row Click event handler
  const handleRowClick = (row) => {
      handleCloseModal(row);
  };


  return (
    <div className="menu-background">
      <div className="wrapper-container">

          {/* Modal component -  */}
          <MedicalAidOptionViewModal
              isOpen={isModalOpen}
              onClose={handleCloseModal}
              title="Medical Aid Options"
              data={modalData}/>

          <div className="singular-staff-heading-container">
            Deductions

            <div className="right-controls">
              <div className="large-box">
                  Date
              </div>
              <div className="small-box">
                  Time
              </div>
            </div>
          </div>

          <div className="cm-navbar-container">
            {pageTabs.map((tab) => (
              <div
                key={tab.value}
                className={`heading-item ${activeTab === tab.value ? "Selected" : ""}`}
                onClick={() => {
                  setActiveTab(tab.value)
                  setActivePage(1);
                }}
              >
                  {tab.label}
              </div>
            ))}
          </div>

          <div className="dynamic-grid-container">
              <DynamicGrid
                  data={currentItems}
                  columns={columns}
                  loading={loading}
                  error={error}
                  currentPage={activePage}
                  totalPages={totalPages}
                  onPageChange={setActivePage}
              />
          </div>
      </div>
    </div>
  );
};

export default MedicalAidOptionsManagement;