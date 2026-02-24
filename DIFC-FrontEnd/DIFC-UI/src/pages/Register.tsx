import { Button, TextInput, PasswordInput, Stack, Title } from "@mantine/core";
import { useForm } from "@mantine/form";
import { api } from "../services/api";

function Register() {
  const form = useForm({
    initialValues: {
      userName: "",
      email: "",
      password: "",
      confirmPassword: "",
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    try {
      await api.post("/auth/register", values);
      alert("User registered successfully!");
    } catch (error: any) {
        console.log("FULL ERROR:", error);
        console.log("SERVER RESPONSE:", error.response?.data);
        alert(JSON.stringify(error.response?.data));
    }
  };

  return (
    <Stack align="center" mt="xl">
      <Title order={2}>Register</Title>

      <form onSubmit={form.onSubmit(handleSubmit)} style={{ width: 300 }}>
        <Stack>
          <TextInput label="User Name" {...form.getInputProps("userName")} />
          <TextInput label="Email" {...form.getInputProps("email")} />
          <PasswordInput label="Password" {...form.getInputProps("password")} />
          <PasswordInput
            label="Confirm Password"
            {...form.getInputProps("confirmPassword")}
          />
          <Button type="submit">Register</Button>
        </Stack>
      </form>
    </Stack>
  );
}

export default Register;