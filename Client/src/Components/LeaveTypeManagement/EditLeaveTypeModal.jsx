import { useState, useEffect } from "react";
import "./add-leave-type-modal.css";
import { updateLeaveType } from "../../api/leaveTypeApi";
import AnnualLeaveEditor from "./AnnualLeaveEditor";

const EditLeaveTypeModal = ({
  isOpen,
  onClose,
  leaveTypes,
  onSuccess,
  selectedId,
  isViewMode
}) => {

  const [selected, setSelected] = useState(null);
  const [isEditing, setIsEditing] = useState(false);
  

  const [form, setForm] = useState({
    description: "",
    femaleOnly: false,
    days: ""
  });

useEffect(() => {
  setIsEditing(!isViewMode);
}, [isViewMode]);

  // LOAD SELECTED LEAVE TYPE
  useEffect(() => {
    if (!selectedId) return;

    const lt = leaveTypes.find(x => x.id === Number(selectedId));
    setSelected(lt);

    if (lt && lt.code !== "AL") {
      const uniqueDays = [...new Set(lt.rules.map(r => r.daysAllocated))];

      setForm({
        description: lt.description || "",
        femaleOnly: lt.femaleOnly,
        days: uniqueDays.length === 1 ? uniqueDays[0] : ""
      });
    }

  }, [selectedId, leaveTypes]);

  if (!isOpen) return null;

  //HANDLE INPUT CHANGE
  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;

    setForm({
      ...form,
      [name]: type === "checkbox" ? checked : value
    });
  };
 if (!form.days || isNaN(form.days)) {
  alert("Please enter valid leave days");
  return;
}
  // SAVE NON-ANNUAL
  const handleSubmit = async () => {
    try {
      await updateLeaveType(selected.id, {
        name: selected.name,
        description: form.description,
        femaleOnly: form.femaleOnly,
       rules: selected.rules.map(r => ({
            groupKey: r.groupKey,
            minYearsService: r.minYearsService,
            maxYearsService: r.maxYearsService,
            daysAllocated:
              form.days !== "" && !isNaN(form.days)
                ? parseFloat(form.days)
                : r.daysAllocated
          }))
      });

      onSuccess();
      onClose();

    } catch (err) {
      console.error("Update failed:", err);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>

        {/* LOGO */}
        <div className="logo-container">
          <span className="logo-bold">singular</span>
          <span className="logo-light">express</span>
        </div>

        <h2 className="modal-title">
  {isEditing
    ? `Edit ${selected?.name}`
    : `View ${selected?.name}`
  }
</h2>

        {/* ================= NON-ANNUAL ================= */}
        {selected && selected.code !== "AL" && (
  <>
    <input value={selected.name} disabled />
    <input value={selected.code} disabled />

    <input
      name="description"
      value={form.description}
      onChange={handleChange}
      placeholder="Description"
      disabled={!isEditing}
    />

    <input
      name="days"
      value={form.days}
      onChange={handleChange}
      placeholder="Leave Entitlement"
      disabled={!isEditing}
    />

    <label className="checkbox">
      <input
        type="checkbox"
        name="femaleOnly"
        checked={form.femaleOnly}
        onChange={handleChange}
        disabled={!isEditing}
      />
      Female Only
    </label>

    <div className="actions">
      {!isEditing ? (
        <>
          <button className="cancel" onClick={onClose}>
            Back
          </button>

          <button className="next" onClick={() => setIsEditing(true)}>
            Edit
          </button>
        </>
      ) : (
        <>
          <button className="cancel" onClick={() => setIsEditing(false)}>
            Cancel
          </button>

          <button className="next" onClick={handleSubmit}>
            Save Changes
          </button>
        </>
      )}
    </div>
  </>
)}

        {/* ================= ANNUAL LEAVE ================= */}
        {selected && selected.code === "AL" && (
          <AnnualLeaveEditor
            leaveType={selected}
            onSuccess={onSuccess}
            onClose={onClose}
            isEditing={isEditing}
            setIsEditing={setIsEditing}
          />
        )}

      </div>
    </div>
  );
};

export default EditLeaveTypeModal;