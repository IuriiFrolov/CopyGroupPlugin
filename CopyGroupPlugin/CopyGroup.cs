using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyGroupPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CopyGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) // добирается до документа, сообщение при неудачи, набор элементов
        { // получили доступ к документу
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            //попросили пользователя выбрать группу объектов
            Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, "Выберите группу объектов"); //  ObjectType- перечисление Nothing	Nothing -пустое Element- элемент PointOnElement - точка на элементе Edge - ребро Face  -грань LinkedElement- Элементы в связанных файлах RVT Subelement - связанные элементы
            Element element = doc.GetElement(reference);
            Group group = element as Group; // предпочтительнее чем преобразование (Group)element в случае исключения передается null

            XYZ point = uiDoc.Selection.PickPoint("Выберите точку");

            Transaction transaction = new Transaction(doc);
            transaction.Start("Копирование группы объектов"); //  doc.Create. и выбрать из https://www.revitapidocs.com/2022/37523148-0dd8-7a2a-8ce9-220095429dd9.htm
            doc.Create.PlaceGroup(point, group.GroupType);
            transaction.Commit();

            return Result.Succeeded;
        }
    }
}
