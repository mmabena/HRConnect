import {useEffect, useState, useCallback} from 'react';
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
import formatToLocalCurrency from "../../../../../utils/formatToLocalCurrency";
import formatSalaryBracket from "../../../../../utils/formatSalaryBracket";
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

    // Handler to open modal || Modified to cater for passing on the options plus it's categories options
    const handleViewRecord = (rowData) => {
      // Get all options in the same category as the selected row
      const categoryId = rowData.medicalOptionCategoryId;
      const categoryOptions = medicalOptions.filter(
        opt => opt.medicalOptionCategoryId === categoryId
      );

      setModalData(categoryOptions || []);
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
      {
        header: "Category", key: "medicalOptionCategoryName", width: 1,
          render: (value, row) => {
            const medicalCategoryId = row.medicalOptionCategoryId;
            // If no category ID or no categories loaded , return N/A
            if (!medicalCategoryId || medicalOptionsCategory.length === 0) {
                return "N/A";
            }

            // Lookup the category by matching the ID (Linq-like query)
            const category = medicalOptionsCategory.find(
                cat => cat.medicalOptionCategoryId === medicalCategoryId
            );

            //Return the category name if found, otherwise N/A
            return category ? category.medicalOptionCategoryName.toUpperCase() : "N/A";
          }
      },
      {
        header: "Option Name", key: "medicalOptionName", width: 1,
          render: (value, row) => {
            return row.medicalOptionName;
          }
      },
      // Custom render for salary bracket/category
      {
        header: "Income Category Salary", key: "salaryBracket", width: 2,
          render: (value, row) => {
            const min = row.salaryBracketMin;
            const max = row.salaryBracketMax;

            const newFormat = formatSalaryBracket(min,max, formatToLocalCurrency);

            return newFormat;
          }
      },
      {
        header: "Principal", key: "totalMonthlyContributionsPrincipal", width:1 ,
          render: (value, row) => {
            const principalAmount = row.totalMonthlyContributionsPrincipal;

            if (principalAmount === null || principalAmount === undefined){
              return formatToLocalCurrency(row.totalMonthlyContributionsAdult, "en-ZA");
            }

            //else return the principal value
            return formatToLocalCurrency(principalAmount, "en-ZA");
          }
      },
      {
        header: "Adult", key: "totalMonthlyContributionsAdult", width:1 ,
          render: (value) => formatToLocalCurrency(value,"en-ZA")
      },
      {
        header: "1st Child", key: "totalMonthlyContributionsChild", width:1 ,
          render: (value) => formatToLocalCurrency(value,"en-ZA")
      },
      {
        header: "2nd Child +", key: "totalMonthlyContributionsChild2", width:1 ,
          render: (value, row) => {
            const amount = row.totalMonthlyContributionsChild2;

            if(amount === null)
            {
              return formatToLocalCurrency(row.totalMonthlyContributionsChild, "en-ZA");
            }
            if(amount === 0)
            {
              return "FREE";
            }

            return formatToLocalCurrency(amount, "en-ZA");
          }
      },
      {
        header: "Actions", key: "actions", width:2,
          render: (value, row) => (
            <div className='edicalaid-options-actions-container'>
              <button
                className="medicalaid-options-borderless-button"
                onClick={
                  () => handleViewRecord(row)
                }
              >
                View | Edit
              </button>
            </div>
          )
        },
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
   
    const handleMedicalUpdateSave = useCallback( async (categoryId, payload) => {
      let statusFlag = false;
      try{
        let requestPayload = await updateBulkMedicalOptionsByCategoryId(categoryId, payload);
        console.log("||--------------------------------< Debug : Update API Response Dump Test From Medical Aid Management Component >-----------------------------------||"); 
	    console.log("<---------Response returned :-----------> ");
	    console.log("Request Metadata : ${request}");

        /*if([200,204,201].includes(request.status)){
          handleCloseModal();
	  const refreshed = await getMedicalOptionsSnapshot();
	  setMedicalOptions(refreshed);
	  statusFlag = true;
	  return (request, statusFlag);	*/
	        //}
	   /* else{
          toast.error('Failed to update options');
	  //return (request,statusFlag);

	    }*/
        console.log(requestPayload);
      }
      catch (error){
        toast.error('Failed to update options: ${error}');
	console.error('Error updating options : ', error);
     
      }
    }, [handleCloseModal, getMedicalOptionsSnapshot, setMedicalOptions]);
    
    return (
      <div className="menu-background">
        <div className="wrapper-container">
    
            {/* Modal component -  */}


            <MedicalAidOptionViewModal
                isOpen={isModalOpen}
                onClose={handleCloseModal}
                title="Medical Aid Options"
                data={modalData}
                categories={medicalOptionsCategory}
                onSave={handleMedicalUpdateSave}
            />

    
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
