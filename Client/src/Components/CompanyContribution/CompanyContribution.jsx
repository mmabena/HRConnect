import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { ToastContainer, toast } from "react-toastify";
import Modal from 'react-modal';
import { 
  fetchLatestChange, 
  downloadGroupLifeContributions, 
  updateBenefits 
} from "./CompanyContributionServices";
import "../../Navy.css";
import "react-toastify/dist/ReactToastify.css";
Modal.setAppElement('#root');


const CompanyContribution = () => {
  const navigate = useNavigate();
  const [showDropdown, setShowDropdown] = useState(false);
  const [latestChange, setLatestChange] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editingRow, setEditingRow] = useState(null);
  const [showConfirmModal, setShowConfirmModal] = useState(false);
  const [rowToEdit, setRowToEdit] = useState(null);
  const [editValues, setEditValues] = useState({
    newDeathBenefitPercentage: "",
    newDisabilityCoverPercentage: "",
  });

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const change = await fetchLatestChange();
      setLatestChange(change);
      setEditValues({
        newDeathBenefitPercentage: change.newDeathBenefitPercentage,
        newDisabilityCoverPercentage: change.newDisabilityCoverPercentage,
      });
    } catch (error) {
      toast.error("Failed to load data");
    } finally {
      setLoading(false);
    }
  };

  const handleRefresh = () => loadData();

  const handleDownload = async () => {
    try {
      await downloadGroupLifeContributions();
      setShowDropdown(false);
    } catch (error) {
      toast.error("Failed to download spreadsheet");
    }
  };

  const handleInputChange = (e, field) => {
    setEditValues(prev => ({
      ...prev,
      [field]: e.target.value,
    }));
  };

  const handleSaveClick = (row) => {
    setRowToEdit(row);
    setShowConfirmModal(true);
  };

const performSave = async () => {
  setShowConfirmModal(false);
  try {
    // Debug: Check localStorage contents before update
    console.log('LocalStorage before update:', {
      token: localStorage.getItem('token'),
      user: localStorage.getItem('user'),
      employeeNumber: localStorage.getItem('employeeNumber')
    });

    // Debug: Check the edit values being sent
    console.log('Edit values being sent:', editValues);

    await updateBenefits(editValues);
    const change = await fetchLatestChange();
    setLatestChange(change);
    setEditingRow(null);
    toast.success("Benefits updated and contributions recalculated successfully!");
  } catch (error) {
    console.error('Update error details:', {
      message: error.message,
      response: error.response?.data,
      stack: error.stack
    });
    
    if (error.message.includes('session data') || error.message.includes('login')) {
      toast.error("Session expired. Please login again.");
      // Optional: Redirect to login page
      // navigate('/login');
    } else {
      toast.error("Failed to update benefits. Please try again.");
    }
  }
};
  const handleCancel = () => {
    setEditValues({
      newDeathBenefitPercentage: latestChange.newDeathBenefitPercentage,
      newDisabilityCoverPercentage: latestChange.newDisabilityCoverPercentage,
    });
    setEditingRow(null);
  };

  return (
    <div className="management-container">
      <Modal
        isOpen={showConfirmModal}
        onRequestClose={() => setShowConfirmModal(false)}
        contentLabel="Confirm Update"
        className="confirmation-modal"
        overlayClassName="modal-overlay"
      >
        <h3>Confirm Benefit Update</h3>
        <p>Are you sure you want to update this benefit percentages?</p>
        <p>
          <strong>Death Benefit:</strong> {editValues.newDeathBenefitPercentage}%<br />
        </p>
        <div className="modal-button-group">
          <button 
            className="modal-confirm-button"
            onClick={performSave}
          >
            Yes, Update Benefit
          </button>
          <button
            className="modal-cancel-button"
            onClick={() => setShowConfirmModal(false)}
          >
            Cancel
          </button>
        </div>
      </Modal>

      <div className="top-bar">
        <h2 className="position-management-title">Company Contribution</h2>
      </div>

      <div className="position-navbar-bar">
        <div className="button-group">
          <button className="refresh-button" onClick={handleRefresh}>
            Refresh Data
          </button>
          <div className="dropdown-wrapper">
            <button
              className="download-button"
              onClick={() => setShowDropdown(prev => !prev)}
            >
              Download Sheet
            </button>
            {showDropdown && (
              <div className="dropdown-menu">
                <button className="dropdown-item" onClick={handleDownload}>
                  Download GroupLife Spreadsheet
                </button>
              </div>
            )}
          </div>
        </div>
      </div>

      <div className="table-wrapper">
        <table className="position-table">
          <thead>
            <tr>
              <th>Contribution Type</th>
              <th>Contribution Percentage</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan="3">Loading...</td>
              </tr>
            ) : (
              <>
                <ContributionRow
                  type="Death Benefit"
                  field="newDeathBenefitPercentage"
                  editing={editingRow === 0}
                  value={editValues.newDeathBenefitPercentage}
                  displayValue={`${latestChange?.newDeathBenefitPercentage ?? 0.565}%`}
                  onChange={handleInputChange}
                  onEdit={() => setEditingRow(0)}
                  onSave={() => handleSaveClick(0)}
                  onCancel={handleCancel}
                />
                <ContributionRow
                  type="Disability Cover"
                  field="newDisabilityCoverPercentage"
                  editing={editingRow === 1}
                  value={editValues.newDisabilityCoverPercentage}
                  displayValue={`${latestChange?.newDisabilityCoverPercentage ?? 0.482}%`}
                  onChange={handleInputChange}
                  onEdit={() => setEditingRow(1)}
                  onSave={() => handleSaveClick(1)}
                  onCancel={handleCancel}
                />
              </>
            )}
          </tbody>
        </table>
      </div>
      <ToastContainer position="top-right" autoClose={5000} />
    </div>
  );
};

const ContributionRow = ({
  type,
  field,
  editing,
  value,
  displayValue,
  onChange,
  onEdit,
  onSave,
  onCancel
}) => (
  <tr>
    <td>{type}</td>
    <td>
      {editing ? (
        <input
          type="number"
          step="0.001"
          min="0"
          value={value}
          onChange={(e) => onChange(e, field)}
        />
      ) : (
        displayValue
      )}
    </td>
    <td>
      {editing ? (
        <div className="action-button-group">
          <button className="action-button save-button" onClick={onSave}>
            Save
          </button>
          <button className="action-button can-button" onClick={onCancel}>
            Cancel
          </button>
        </div>
      ) : (
        <button className="action-button edit-button" onClick={onEdit}>
          Edit
        </button>
      )}
    </td>
  </tr>
);

export default CompanyContribution;