import { useState, useEffect, useCallback } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { getCompany, deleteCompany, getCompanyById } from './Components/Services/companyService.js';
import CompanyManagementUI from './Components/companyManagement/companyManagementUI.jsx';

function CompanyManagement() {
  const navigate = useNavigate();
  const location = useLocation();
  const queryParams = new URLSearchParams(location.search);
  const initialTab = queryParams.get('tab') || 'Tax Table Management';

  const [companies, setCompanies] = useState([]);
  const [selectedCompanyId, setSelectedCompanyId] = useState('');
  const [viewedCompanyDetails, setViewedCompanyDetails] = useState(null);

  useEffect(() => {
    const fetchCompanies = async () => {
      try {
        const response = await getCompany();
         console.log("Fetched companies response:", response);
        setCompanies(response.data);
       
      } catch (error) {
        console.error('Error fetching companies:', error);
        alert('Failed to load companies.');
      }
    };
    fetchCompanies();
  }, []);

  useEffect(() => {

    async function fetchViewedCompany() {
      if (selectedCompanyId) {
        try {
          const response = await getCompanyById(selectedCompanyId);
          if (response.data) {
            setViewedCompanyDetails(response.data);
          } else {
            alert("Selected company not found.");
            setViewedCompanyDetails(null);
          }
        } catch (err) {
          alert("Failed to load selected company data.");
          console.error("Error fetching selected company:", err);
          setViewedCompanyDetails(null);
        }
      } else {
        setViewedCompanyDetails(null);
      }
    }
    fetchViewedCompany();
  }, [selectedCompanyId]);

  const handleEditCompanyDetails = useCallback((companyId) => {
    navigate(`/editCompany/${companyId}`);
  }, [navigate]);

  const handleDeleteCompany = useCallback(async (companyId) => {
    if (window.confirm('Are you sure you want to delete this company?')) {
      try {
        await deleteCompany(companyId);
        setCompanies(companies.filter((company) => company.companyId !== companyId));
        alert('Company deleted successfully!');
      } catch (error) {
        console.error('Error deleting company:', error);
        alert('Failed to delete company.');
      }
    }
  }, [companies]);

  const handleCompanySelectChange = useCallback((e) => {
    setSelectedCompanyId(e.target.value);
  }, []);


  const handlers = {
    handleEditCompanyDetails,
    handleDeleteCompany,
    handleCompanySelectChange, 
  };

  const config = {
    initialTab,
  };

  return (
    <CompanyManagementUI
      companies={companies}
      selectedCompanyId={selectedCompanyId}
      viewedCompanyDetails={viewedCompanyDetails}
      handlers={handlers} 
      config={config} 
    />
  );
}

export default CompanyManagement;