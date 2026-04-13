import { useState, useEffect } from "react";
import "./add-leave-type-modal.css";
import { updateLeaveType } from "../../api/leaveTypeApi";
import AnnualLeaveEditor from "./AnnualLeaveEditor";

const EditLeaveTypeModal = ({
  isOpen,
  onClose,
  leaveTypes,
  onSuccess,
  selectedId: initialSelectedId 
}) => {

  const [selectedId, setSelectedId] = useState(initialSelectedId || "");
  const [selected, setSelected] = useState(null);

  const [form, setForm] = useState({
    description: "",
    femaleOnly: false,
    days: ""
  });



  // 🔥 when dropdown changes
  useEffect(() => {
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
  useEffect(() => {
  if (initialSelectedId) {
    setSelectedId(initialSelectedId);
  }
}, [initialSelectedId]);
    if (!isOpen) return null;
  const handleSubmit = async () => {
  try {
    await updateLeaveType(selected.id, {
      name: selected.name, // required by backend
      description: form.description,
      femaleOnly: form.femaleOnly,
      rules: [
        {
          jobGradeId: null,
          minYearsService: 0,
          maxYearsService: null,
          daysAllocated: parseFloat(form.days)
        }
      ]
    });

    onSuccess(); // refresh table
    onClose();   // close modal

  } catch (err) {
    console.error("Update failed:", err);
  }
};

  // 🔥 handle form changes
  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm({
      ...form,
      [name]: type === "checkbox" ? checked : value
    });
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>

        {/* LOGO */}
        <div className="logo-container">
          <span className="logo-bold">singular</span>
          <span className="logo-light">express</span>
        </div>

        <h2 className="modal-title">Edit Leave Type</h2>

        {/* 🔥 DROPDOWN */}
        <select
          className="modal-dropdown"
          value={selectedId}
          onChange={(e) => setSelectedId(e.target.value)}
        >
          <option value="">Select Leave Type</option>
          {leaveTypes.map(lt => (
            <option key={lt.id} value={lt.id}>
              {lt.name}
            </option>
          ))}
        </select>

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
            />

            <input
              name="days"
              value={form.days}
              onChange={handleChange}
              placeholder="Leave Entitlement"
            />

            <label className="checkbox">
              <input
                type="checkbox"
                name="femaleOnly"
                checked={form.femaleOnly}
                onChange={handleChange}
              />
              Female Only
            </label>

            <button className="modal-btn" onClick={handleSubmit}>
              Save
            </button>
          </>
        )}

        {/* ================= ANNUAL LEAVE ================= */}
        {selected && selected.code === "AL" && (
          <AnnualLeaveEditor leaveType={selected} />
        )}

      </div>
    </div>
  );
};

export default EditLeaveTypeModal;