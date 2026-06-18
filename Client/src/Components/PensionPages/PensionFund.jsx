import React, { useEffect, useState } from "react";
import {
  getPensionFunds,
  getPensionOptions,
  addPensionFund,
  updatePensionOption,
  createPensionOption,
  deletePensionFund,
  deleteAllPensionOptions
} from "../../api/PensionFund";

import "./PensionFund.css";
import PayrollNavbar from "../PayrollNavbar";

export default function PensionFundsList({isOpen, onClose}) {
  // STATE 
  const [funds, setFunds] = useState([]);
  const [options, setOptions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [editingFund, setEditingFund] = useState(null);
  const [editingOption, setEditingOption] = useState(null);
  const [optionPercentage, setOptionPercentage] = useState("");
  // FORM STATE
  const [showForm, setShowForm] = useState(false);
  const [name, setName] = useState("");
  const [taxCode, setTaxCode] = useState("");
  const [description, setDescription] = useState("");
  const [formError, setFormError] = useState("");
  const [inactiveFunds, setInactiveFunds] = useState([]);


  // FETCH DATA 
  useEffect(() => {
    fetchFunds();
    fetchOptions();
  }, []);

  const fetchFunds = async () => {
  try {
    const res = await getPensionFunds();

    const safeData =
    Array.isArray(res)
        ? res
        : Array.isArray(res?.data)
        ? res.data
        : Array.isArray(res?.$values)
        ? res.$values
        : [];

    const activeFunds = safeData.filter((f) => f.isActive);
    const inactiveFundsList = safeData.filter((f) => !f.isActive);

    setFunds(activeFunds);
    setInactiveFunds(inactiveFundsList);

  } catch (err) {
    console.error(err);
    setError("Failed to load pension funds");
    setFunds([]);
    setInactiveFunds([]);
  }
};


  const fetchOptions = async () => {
  try {
      const res = await getPensionOptions();

      const safeOptions =
        Array.isArray(res)
          ? res
          : Array.isArray(res?.data)
          ? res.data
          : Array.isArray(res?.$values)
          ? res.$values
          : [];

      setOptions(safeOptions);

    } catch (err) {
      console.error(err);
      setError("Failed to load pension options");
      setOptions([]);
    } finally {
      setLoading(false);
    }
  };

  // FORM HANDLERS 

  const handleAddClick = () => {

    setEditingFund(null);
    setName("");
    setTaxCode("");
    setDescription("");
    setFormError("");
    setShowForm(true);

  };

  const handleEditClick = (fund) => {
    setEditingFund(fund);
    setName(fund.name || "");
    setTaxCode(fund.taxCode || "");
    setDescription(fund.description || "");
    setFormError("");
    setShowForm(true);
  };

  const handleCloseForm = () => {

    setShowForm(false);
    setEditingFund(null);
    setEditingOption(null);
    setFormError("");
    setOptionPercentage("");
  };

  // SAVE FUND 
 const handleFormSubmit = async (e) => {
  e.preventDefault();

  const payload = { name, taxCode, description };

  try {
    const result = await addPensionFund(payload); // API call

    if (!result.isSuccess) {
      setFormError(result.message);  
      return;
    }

    await fetchFunds();
    setShowForm(false);
    setEditingFund(null);
    setName("");
    setTaxCode("");
    setDescription("");
    setFormError("");
    onClose();
  } catch (err) {
    console.error(err);
    setFormError("Failed to save pension fund.");
  }
};


return (
  <div className="menu-background custom-scrollbar" >
  <div className="wrap-container">
  <div className="heading-container2">Payroll Management</div>
  <h2 className="sub-heading">Pension Fund Management</h2>
  </div>



  <div className="add-button-wrapper">
    <h2 className="inactive-title">Active Pension Funds</h2>
  <button className="add-button" onClick={handleAddClick}>
  Add Pension Fund
  </button>
  </div>

  {error && (
  <p className="error-message">
  {error}
  </p>
  )}

  {/* FUNDS TABLE */}
<table className="pension-funds-table">
  <thead>
  <tr>
  <th>Name</th>
  <th>Description</th>
  <th>Tax Code</th>
  <th>Actions</th>
  </tr>
  </thead>

  <tbody>
  {funds.length === 0 ? (
  <tr>
  <td
  colSpan="4"
  className="no-data-row">
  No Pension Fund Found
  </td>
  </tr>
  ) : (
  funds.map((f) => (
  <tr key={f.pensionFundId}>
  <td>{f.name}</td>
  <td>{f.description}</td>
  <td>{f.taxCode}</td>
 
<td className="actions-cell">
  {/* EDIT */}
  <span
    className="table-action-tab"
    onClick={() => handleEditClick(f)}
  >
    Edit
  </span>

  {/* DELETE */}
  <span
    className="delete-tab"
    onClick={async () => {
      const confirmed = window.confirm(
        `De-activate ${f.name} and all its options?`
      );
      if (!confirmed) return;

      try {
        await deleteAllPensionOptions(f.pensionFundId);
        await deletePensionFund(f.pensionFundId);
        await fetchFunds();
        await fetchOptions();
      } catch (err) {
        console.error(err);
        alert("Failed to delete pension fund.");
      }
    }}
  >
    De-activate
  </span>
</td>


  </tr>
  ))
  )}

  </tbody>
  </table>

    {/* OPTIONS TABLE */}
    <div className="pension-container">
      <h2 className="pension-title">
      Pension Options
      </h2>
      {options.length === 0 ? (
      <p className="pension-empty">
      No pension options available.
      </p>
      ) : (

  <table className="pension-options-table">
  <thead>
  <tr>
  <th>Option Name</th>
  <th>Percentage</th>
  </tr>
  </thead>
  <tbody>
  {options.map((option) => (
  <tr key={option.pensionOptionId}>
  <td>{funds[0]?.name} Pension</td>
  <td>{option.contributionPercentage}%</td>
  </tr>
    ))}
 </tbody>
 </table>

  )}
  </div>

<div className="inactive-funds-container">
<h2 className="inactive-title">Inactive Pension Funds</h2>
<table className="styled-table inactive-funds-table">
<thead>
<tr>
<th>Name</th>
<th>Description</th>
<th>Tax Code</th>
<th>Status</th>
</tr>
  </thead>
  <tbody>
  {inactiveFunds.length === 0 ? (
  <tr>
  <td colSpan="4" className="no-data-row">
    No Inactive Pension Funds Found
      </td>
      </tr>
      ) : (
      inactiveFunds.map((f) => (
      <tr key={f.pensionFundId}>
      <td>{f.name}</td>
      <td>{f.description}</td>
      <td>{f.taxCode}</td>
    <td>
<div className="inactive-status-pill">
<span className="status-dot"></span>
Inactive
</div>
</td>
      </tr>
      ))
      )}
</tbody>
</table>
</div>


{showForm && (
<div className="modal-overlay" onClick={handleCloseForm}>
<div className="modal-box" onClick={(e) => e.stopPropagation()}>

{/* ADD FORM */}
{!editingFund && (
<form onSubmit={handleFormSubmit} className="pension-form pension-form-add">

    <div className="logo-container-add">
    <span className="apm-logo-bold">singular</span>
    <span className="apm-logo-light">express</span>
    </div>

    <div className="add-fund-wrapper">
    <span>Add Pension Fund</span>
    </div>

  {/* ERROR MESSAGE */}
  {formError && (
  <div className="form-error">
  {formError}
  </div>
  )}

    {/* NAME */}
    <input
    type="text"
    className="modal-input"
    value={name}
    onChange={(e) => setName(e.target.value)}
    placeholder="Name"
    required
    />

    {/* TAX CODE */}
    <input
    type="text"
    className="modal-input"
    value={taxCode}
    onChange={(e) => setTaxCode(e.target.value)}
    placeholder="Tax Code"
    required
    />

    {/* DESCRIPTION */}
    <input
    type="text"
    className="modal-input"
    value={description}
    onChange={(e) => setDescription(e.target.value)}
    placeholder="Description"
    required
    />

    {/* ACTION BUTTONS */}
    <div className="form-actions">
    <button type="submit" className="btn-save">Save</button>
    </div>

    {/* FOOTER */}
    <div className="pm-footer">
    <p className="footer1">Privacy Policy | Terms & Conditions</p>
    <p className="footer2">Copyright © 2026 Singular Systems. All rights reserved.</p>
    </div>
    </form>
      )}
      
{editingFund && (
  <form className="pension-form pension-form-edit">

    {/* Logo for Edit Form */}
    <div className="logo-container-edit">
    <span className="apm-logo-bold">singular</span>
    <span className="apm-logo-light">express</span>
    </div>

    <div className="add-fund-wrapper">
    <span>Edit Pension Fund</span>
    </div>

    <div className="options-card">

      <input
        type="text"
        className="edit-fund-input"
        value={name}
        onChange={(e) => setName(e.target.value)}
        placeholder="Name"
      />

      <input
        type="text"
        className="edit-fund-input"
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        placeholder="Description"
      />

     
      <h3 className="section-title">Pension Options</h3>

      <table className="pension-options-table">
      <thead>
      <tr>
     <th>Option</th>
     <th>Percentage</th>
     <th>Action</th>
      </tr>
      </thead>
<tbody>
  {options.map((opt) => (
  <tr key={opt.pensionOptionId}>
  <td>{editingFund.name} Pension</td>
  <td>
  {editingOption?.pensionOptionId === opt.pensionOptionId ? (
  <input
  type="number"
  className="edit-option-input"
  value={opt.contributionPercentage}
  onChange={(e) => {
  const updatedOptions = options.map(o =>
  o.pensionOptionId === opt.pensionOptionId
  ? { ...o, contributionPercentage: Number(e.target.value) }
  : o
  );
  setOptions(updatedOptions);
  }}
  />
  ) : (
  `${opt.contributionPercentage}%`
  )}
  </td>
  <td>
  {editingOption?.pensionOptionId === opt.pensionOptionId ? (
  <span className="editing-label">Editing…</span>
  ) : (
  <span
  className="clickable-tab"
  onClick={() => {
  setEditingOption(opt);
  }}
  >
  Edit
  </span>
  )}
  </td>
  </tr>
  ))}
</tbody>

</table>

<button
  type="button"
  className="btn-add-option"
  onClick={async () => {
  try {
  const newOption = {
  contributionPercentage: 0, // default
  };
  const created = await createPensionOption(newOption);
  await fetchOptions();                  
  setEditingOption(created);             
  setOptionPercentage(created.contributionPercentage);
  } catch (err) {
  console.error(err);
  alert("Failed to add option.");
  }
  }}
>
  <svg xmlns="http://www.w3.org/2000/svg" 
  viewBox="0 0 24 24" 
  width="24" height="24" 
  className="plus-icon">
  <path d="M12 5v14M5 12h14" />
  </svg>
  Add Option
</button>


      {/* SAVE FUND */}
  <div className="edit-form-actions">
  <button
  type="button"
  className="edit-form-back-btn"
  onClick={async () => {
  try {
  for (const opt of options) {
  await updatePensionOption(opt);
  }
  await fetchOptions();
  alert("All edited options saved successfully.");
  setEditingOption(null);
  } catch (err) {
  console.error(err);
  alert("Failed to save options.");
  }
  }}
  >
  Save
  </button>
  </div>

    </div>

  </form>
)}

    </div>
  </div>
  )}
   </div>
  )}
