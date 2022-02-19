using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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
        {
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;
                GroupPickFilter groupPickFilter = new GroupPickFilter();
                //попросили пользователя выбрать группу объектов
                Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, groupPickFilter, "Выберите группу объектов"); //  ObjectType- перечисление Nothing	Nothing -пустое Element- элемент PointOnElement - точка на элементе Edge - ребро Face  -грань LinkedElement- Элементы в связанных файлах RVT Subelement - связанные элементы
                Element element = doc.GetElement(reference);
                Group group = element as Group; // предпочтительнее чем преобразование (Group)element в случае исключения передается null
                XYZ groupCenter = GetElementCenter(group); // Нашли центр группы возвав к нашему классу 
                Room room = GetRoomByPoint(doc, groupCenter); // Находим к какой комнате пренадлежит выбранная за основу группа
                XYZ roomCenter = GetElementCenter(room);  // Находим центр комнаты в которой мы выделили исходную группу
                XYZ offset = groupCenter - roomCenter;   // Находим смещение центра группы оносительно центра комнаты


                XYZ userPoint = uiDoc.Selection.PickPoint("Выберите точку");
                Room room2 = GetRoomByPoint(doc, userPoint);
                XYZ roomCenter2 = GetElementCenter(room2);
                XYZ insertionPoint = offset + roomCenter2;

                Transaction transaction = new Transaction(doc);
                transaction.Start("Копирование группы объектов"); //  doc.Create. и выбрать из https://www.revitapidocs.com/2022/37523148-0dd8-7a2a-8ce9-220095429dd9.htm
                doc.Create.PlaceGroup(insertionPoint, group.GroupType);
                transaction.Commit();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch(Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public XYZ GetElementCenter(Element element)
        {
            BoundingBoxXYZ bounding = element.get_BoundingBox(null); // Свойство Min левый нижний дальний угол
            return (bounding.Max + bounding.Min) / 2;
        }

        public Room GetRoomByPoint(Document doc, XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);
            foreach(Element e in collector)
            {
                Room room = e as Room;
                if (room!=null)
                {
                    if (room.IsPointInRoom(point))
                    {
                        return room;
                    }
                }
            }
            return null;
        }
    }
}
public class GroupPickFilter : ISelectionFilter
{
    public bool AllowElement(Element elem)
    {
        if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
            return true;
        else
            return false;
    }

    public bool AllowReference(Reference reference, XYZ position)
    {
        return false;
    }
}
