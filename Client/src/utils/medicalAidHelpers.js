export const getDependentCounts = (deps = []) => {
  let principalCount = 1;
  let adultCount = 0;
  let childrenCount = 0;

  deps.forEach((d) => {
    const rel = (d.relationship || "").toLowerCase();

    if (rel === "spouse" || rel === "parent" || rel === "sibling") {
      adultCount++;
    } else if (rel === "child") {
      childrenCount++;
    } else {
      adultCount++;
    }
  });

  return {
    principal: principalCount,
    adult: adultCount,
    childrenCount,
  };
};


export const populateDependentFromIdNumber = (idNumber) => {
  if (!idNumber || idNumber.toString().length !== 13) {
    return {};
  }

  try {
    const idStr = idNumber.toString().trim();

    // YYMMDD
    const yearPart = parseInt(idStr.slice(0, 2), 10);
    const month = parseInt(idStr.slice(2, 4), 10);
    const day = parseInt(idStr.slice(4, 6), 10);

    const fullYear = yearPart < 30 ? 2000 + yearPart : 1900 + yearPart;

    const dob = new Date(fullYear, month - 1, day);

    // Invalid date check
    if (
      dob.getFullYear() !== fullYear ||
      dob.getMonth() !== month - 1 ||
      dob.getDate() !== day
    ) {
      return {};
    }

    // Gender digits (7th-10th digits)
    const genderDigits = parseInt(idStr.slice(6, 10), 10);

    const gender = genderDigits >= 5000
      ? "Male"
      : "Female";

    return {
      dateOfBirth: dob.toLocaleDateString("en-CA"),
      gender,
    };
  } catch (error) {
    console.error("Error parsing dependent ID number:", error);
    return {};
  }
};