import { useState } from "react";
import axios from "axios";

const useImageUpload = () => {

  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState("");

  const uploadImage = async (file) => {

    if (!file) return null;

    if (!["image/jpeg","image/jpg","image/png"].includes(file.type)) {
      setError("Only jpg, jpeg, png allowed");
      return null;
    }

    try {

      setUploading(true);

      const formData = new FormData();
      formData.append("file", file);
      formData.append("upload_preset", "unsigned_preset");

      const response = await axios.post(
        "https://api.cloudinary.com/v1_1/djmafre5k/image/upload",
        formData
      );

      return response.data.secure_url;

    } catch (err) {

      setError("Upload failed");
      return null;

    } finally {

      setUploading(false);

    }
  };

  return { uploadImage, uploading, error };

};

export default useImageUpload;