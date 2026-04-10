import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import type { ReactNode } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import {
  itemConditionLabels,
  itemConditions,
  itemStatusLabels,
  itemStatuses,
  type SaveItemPayload,
} from "@/features/items/types";

const schema = z.object({
  title: z.string().trim().min(1, "Title is required.").max(150, "Keep the title under 150 characters."),
  brand: z.string().trim().min(1, "Brand is required.").max(100, "Keep the brand under 100 characters."),
  model: z.string().trim().min(1, "Model is required.").max(100, "Keep the model under 100 characters."),
  category: z.string().trim().min(1, "Category is required.").max(100, "Keep the category under 100 characters."),
  condition: z.enum(itemConditions),
  description: z.string().trim().min(1, "Description is required.").max(4000, "Keep the description under 4000 characters."),
  askingPrice: z.coerce.number().min(0, "Price must be zero or more."),
  status: z.enum(itemStatuses),
});

type FormValues = z.infer<typeof schema>;

const defaultValues: FormValues = {
  title: "",
  brand: "",
  model: "",
  category: "Electronics",
  condition: "Good",
  description: "",
  askingPrice: 0,
  status: "Draft",
};

type ItemEditorFormProps = {
  initialValues?: SaveItemPayload;
  submitLabel: string;
  isSubmitting: boolean;
  onSubmit: (values: SaveItemPayload) => void;
};

export function ItemEditorForm({
  initialValues,
  submitLabel,
  isSubmitting,
  onSubmit,
}: ItemEditorFormProps) {
  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: initialValues ?? defaultValues,
  });

  useEffect(() => {
    if (initialValues) {
      form.reset(initialValues);
    }
  }, [form, initialValues]);

  return (
    <form
      className="space-y-6"
      onSubmit={form.handleSubmit((values) => {
        onSubmit({
          ...values,
          title: values.title.trim(),
          brand: values.brand.trim(),
          model: values.model.trim(),
          category: values.category.trim(),
          description: values.description.trim(),
        });
      })}
    >
      <div className="grid gap-5 md:grid-cols-2">
        <Field error={form.formState.errors.title?.message} htmlFor="title" label="Title">
          <Input id="title" placeholder="MacBook Air M2 13-inch" {...form.register("title")} />
        </Field>

        <Field error={form.formState.errors.category?.message} htmlFor="category" label="Category">
          <Input id="category" placeholder="Electronics" {...form.register("category")} />
        </Field>

        <Field error={form.formState.errors.brand?.message} htmlFor="brand" label="Brand">
          <Input id="brand" placeholder="Apple" {...form.register("brand")} />
        </Field>

        <Field error={form.formState.errors.model?.message} htmlFor="model" label="Model">
          <Input id="model" placeholder="A2681" {...form.register("model")} />
        </Field>

        <Field error={form.formState.errors.condition?.message} htmlFor="condition" label="Condition">
          <Select id="condition" {...form.register("condition")}>
            {itemConditions.map((condition) => (
              <option key={condition} value={condition}>
                {itemConditionLabels[condition]}
              </option>
            ))}
          </Select>
        </Field>

        <Field error={form.formState.errors.status?.message} htmlFor="status" label="Status">
          <Select id="status" {...form.register("status")}>
            {itemStatuses.map((status) => (
              <option key={status} value={status}>
                {itemStatusLabels[status]}
              </option>
            ))}
          </Select>
        </Field>

        <Field error={form.formState.errors.askingPrice?.message} htmlFor="askingPrice" label="Asking price">
          <Input
            id="askingPrice"
            inputMode="decimal"
            step="0.01"
            type="number"
            {...form.register("askingPrice", { valueAsNumber: true })}
          />
        </Field>
      </div>

      <Field
        error={form.formState.errors.description?.message}
        htmlFor="description"
        label="Description"
      >
        <Textarea
          id="description"
          placeholder="What's included, cosmetic condition, known issues, storage size, battery health, and any accessories."
          {...form.register("description")}
        />
      </Field>

      <div className="flex justify-end">
        <Button disabled={isSubmitting} type="submit">
          {isSubmitting ? "Saving..." : submitLabel}
        </Button>
      </div>
    </form>
  );
}

type FieldProps = {
  children: ReactNode;
  error?: string;
  htmlFor: string;
  label: string;
};

function Field({ children, error, htmlFor, label }: FieldProps) {
  return (
    <div className="space-y-2">
      <Label htmlFor={htmlFor}>{label}</Label>
      {children}
      <p className="min-h-5 text-sm text-red-600">{error ?? ""}</p>
    </div>
  );
}
